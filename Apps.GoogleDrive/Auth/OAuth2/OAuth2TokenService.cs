using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using System.Text.Json;

namespace Apps.GoogleDrive.Auth.OAuth2;

public class OAuth2TokenService : BaseInvocable, IOAuth2TokenService, ITokenRefreshable
{
    private const string ExpiresAtKeyName = "expires_at";
    private const string TokenUrl = "https://oauth2.googleapis.com/token";

    public OAuth2TokenService(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public bool IsRefreshToken(Dictionary<string, string> values)
        => values.TryGetValue(ExpiresAtKeyName, out var expireValue) &&
           DateTime.UtcNow > DateTime.Parse(expireValue);

    public int? GetRefreshTokenExprireInMinutes(Dictionary<string, string> values)
    {
        if (!values.TryGetValue(ExpiresAtKeyName, out var expireValue))
            return null;

        if (!DateTime.TryParse(expireValue, out var expireDate))
            return null;

        var difference = expireDate - DateTime.UtcNow;

        return (int)difference.TotalMinutes - 5;
    }

    public async Task<Dictionary<string, string>> RefreshToken(Dictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        const string grant_type = "refresh_token";

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

        using var httpClient = new HttpClient();
        using var httpContent = new FormUrlEncodedContent(bodyParameters);
        using var response = await httpClient.PostAsync(TokenUrl, httpContent, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new PluginMisconfigurationException($"Google OAuth token request failed. Please reconnect the connection. Details: {responseContent}");

        var resultDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent)
                               ?.ToDictionary(r => r.Key, r => r.Value?.ToString())
                           ?? throw new PluginApplicationException($"Invalid token response: {responseContent}");

        if (!resultDictionary.TryGetValue("access_token", out var token) || string.IsNullOrWhiteSpace(token))
            throw new PluginApplicationException($"Token response does not contain access_token. Response: {responseContent}");

        var expiresIn = int.Parse(resultDictionary["expires_in"]);
        var expiresAt = utcNow.AddSeconds(expiresIn);
        resultDictionary[ExpiresAtKeyName] = expiresAt.ToString("O");

        return resultDictionary!;
    }
}