using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Apps.GoogleDrive.Auth.OAuth2;

public class OAuth2TokenService : BaseInvocable, IOAuth2TokenService, ITokenRefreshable
{
    private const string ExpiresAtKeyName = "expires_at";
    private const string TokenUrl = "https://oauth2.googleapis.com/token";
    private const int RefreshLeadTimeMinutes = 20;

    public OAuth2TokenService(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public bool IsRefreshToken(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
        {
            InvocationContext.Logger?.LogError(
                $"[GoogleDriveOAuth] Refresh decision. ExpiresAt: missing; ShouldRefresh: false; HasAccessToken: {HasValue(values, "access_token")}; HasRefreshToken: {HasValue(values, "refresh_token")}; AccessToken: {GetTokenFingerprint(values, "access_token")}; RefreshToken: {GetTokenFingerprint(values, "refresh_token")}",
                null);
            return false;
        }

        var expiresAt = DateTime.Parse(expireValue);
        var refreshAt = expiresAt.AddMinutes(-RefreshLeadTimeMinutes);
        var utcNow = DateTime.UtcNow;
        var shouldRefresh = utcNow >= refreshAt;

        InvocationContext.Logger?.LogError(
            $"[GoogleDriveOAuth] Refresh decision. CurrentUtc: {utcNow:O}; ExpiresAt: {expiresAt:O}; RefreshAt: {refreshAt:O}; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}; ShouldRefresh: {shouldRefresh}; HasAccessToken: {HasValue(values, "access_token")}; HasRefreshToken: {HasValue(values, "refresh_token")}; AccessToken: {GetTokenFingerprint(values, "access_token")}; RefreshToken: {GetTokenFingerprint(values, "refresh_token")}",
            null);

        return shouldRefresh;
    }

    public int? GetRefreshTokenExprireInMinutes(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
        {
            InvocationContext.Logger?.LogError(
                $"[GoogleDriveOAuth] Refresh schedule unavailable. Reason: expires_at is missing; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}",
                null);
            return null;
        }

        if (!DateTime.TryParse(expireValue, out var expireDate))
        {
            InvocationContext.Logger?.LogError(
                $"[GoogleDriveOAuth] Refresh schedule unavailable. Reason: expires_at is invalid; ExpiresAt: {expireValue}; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}",
                null);
            return null;
        }

        var utcNow = DateTime.UtcNow;
        var difference = expireDate - utcNow;
        var refreshInMinutes = (int)difference.TotalMinutes - RefreshLeadTimeMinutes;

        InvocationContext.Logger?.LogError(
            $"[GoogleDriveOAuth] Refresh schedule calculated. CurrentUtc: {utcNow:O}; ExpiresAt: {expireDate:O}; RefreshAt: {expireDate.AddMinutes(-RefreshLeadTimeMinutes):O}; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}; RefreshInMinutes: {refreshInMinutes}",
            null);

        return refreshInMinutes;
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        const string grant_type = "refresh_token";

        InvocationContext.Logger?.LogError(
            $"[GoogleDriveOAuth] Starting refresh token flow. ExpiresAt: {GetSafeValue(values, ExpiresAtKeyName)}; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}; HasAccessToken: {HasValue(values, "access_token")}; HasRefreshToken: {HasValue(values, "refresh_token")}; AccessToken: {GetTokenFingerprint(values, "access_token")}; RefreshToken: {GetTokenFingerprint(values, "refresh_token")}",
            null);

        var bodyParameters = new Dictionary<string, string>
        {
            { "grant_type", grant_type },
            { "client_id", ApplicationConstants.ClientId },
            { "client_secret", ApplicationConstants.ClientSecret },
            { "refresh_token", values["refresh_token"] },
        };
        return await RequestToken(bodyParameters, cancellationToken);
    }

    public async Task<Dictionary<string, string?>> RequestToken(
        string state,
        string code,
        Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        const string grant_type = "authorization_code";

        var bodyParameters = new Dictionary<string, string>
        {
            { "grant_type", grant_type },
            { "client_id", ApplicationConstants.ClientId },
            { "client_secret", ApplicationConstants.ClientSecret },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "code", code }
        };
        return await RequestToken(bodyParameters, cancellationToken);
    }

    public Task RevokeToken(Dictionary<string, string> values)
    {
        throw new NotImplementedException();
    }

    private async Task<Dictionary<string, string>> RequestToken(Dictionary<string, string> bodyParameters,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var logger = InvocationContext.Logger;

        using var httpClient = new HttpClient();
        using var httpContent = new FormUrlEncodedContent(bodyParameters);

        var grantType = GetSafeValue(bodyParameters, "grant_type");
        var refreshTokenFingerprint = GetTokenFingerprint(bodyParameters, "refresh_token");

        logger?.LogError(
            $"[GoogleDriveOAuth] Requesting OAuth token. GrantType: {grantType}; RefreshToken: {refreshTokenFingerprint}",
            null);

        using var response = await httpClient.PostAsync(TokenUrl, httpContent, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger?.LogError(
                $"[GoogleDriveOAuth] OAuth token request failed. GrantType: {grantType}; StatusCode: {(int)response.StatusCode} {response.StatusCode}; RefreshToken: {refreshTokenFingerprint}; Response: {responseContent}",
                null);

            throw new PluginMisconfigurationException($"Google OAuth token request failed. Details: {responseContent}");
        }

        var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)
                               ?.ToDictionary(r => r.Key, r => r.Value?.ToString())
                           ?? throw new PluginApplicationException($"Invalid token response: {responseContent}");

        if (!resultDictionary.TryGetValue("access_token", out var token) || string.IsNullOrWhiteSpace(token))
        {
            logger?.LogError(
                $"[GoogleDriveOAuth] Access token is missing in the response. GrantType: {grantType}; StatusCode: {(int)response.StatusCode} {response.StatusCode}",
                null);
            throw new PluginApplicationException("Token response does not contain access_token.");
        }

        string? expiresAt = null;
        if (resultDictionary.TryGetValue("expires_in", out var expiresStr) && int.TryParse(expiresStr, out var expiresIn))
        {
            expiresAt = utcNow.AddSeconds(expiresIn).ToString("O");
            resultDictionary[ExpiresAtKeyName] = expiresAt;
        }

        resultDictionary.TryGetValue("refresh_token", out var returnedRefreshToken);
        var expiresInSeconds = resultDictionary.TryGetValue("expires_in", out var expiresInValue)
            ? expiresInValue ?? "missing"
            : "missing";
        logger?.LogError(
            $"[GoogleDriveOAuth] OAuth token request succeeded. GrantType: {grantType}; StatusCode: {(int)response.StatusCode} {response.StatusCode}; ExpiresInSeconds: {expiresInSeconds}; ExpiresAt: {expiresAt ?? "missing"}; RefreshLeadTimeMinutes: {RefreshLeadTimeMinutes}; AccessToken: {GetTokenFingerprint(token)}; RefreshTokenReturned: {!string.IsNullOrWhiteSpace(returnedRefreshToken)}; ReturnedRefreshToken: {GetTokenFingerprint(returnedRefreshToken)}",
            null);
        return resultDictionary;
    }

    private static bool HasValue(Dictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

    private static string GetSafeValue(Dictionary<string, string> values, string key)
        => values.TryGetValue(key, out var value) ? value : "missing";

    private static string GetTokenFingerprint(Dictionary<string, string> values, string key)
        => values.TryGetValue(key, out var token) ? GetTokenFingerprint(token) : "missing";

    private static string GetTokenFingerprint(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return "missing";

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return $"sha256:{Convert.ToHexString(hash)[..12]};len:{token.Length}";
    }
}
