using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication.OAuth2;
using Blackbird.Applications.Sdk.Common.Invocation;
using Microsoft.AspNetCore.WebUtilities;

namespace Apps.GoogleDrive.Auth.OAuth2;

public class OAuth2AuthorizeService : BaseInvocable, IOAuth2AuthorizeService
{
    public OAuth2AuthorizeService(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public string GetAuthorizationUrl(Dictionary<string, string> values)
    {
        string bridgeOauthUrl = $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/oauth";
        const string oauthUrl = "https://accounts.google.com/o/oauth2/v2/auth";

        // Labels admin scope
        var isAdminScopeValid = bool.TryParse(InvocationContext.AuthenticationCredentialsProviders.FirstOrDefault(x => x.KeyName == "useAdminAccess").Value, out var isAdminScope);
        var adminScope = isAdminScopeValid && isAdminScope ? "https://www.googleapis.com/auth/drive.admin.labels" : string.Empty;
        
        var parameters = new Dictionary<string, string>
        {
            { "client_id", ApplicationConstants.ClientId },
            { "redirect_uri", $"{InvocationContext.UriInfo.BridgeServiceUrl.ToString().TrimEnd('/')}/AuthorizationCode" },
            { "response_type", "code" },
            { "scope", ApplicationConstants.Scope + $" {adminScope}" },
            { "state", values["state"] },
            { "access_type", "offline" },
            { "prompt", "consent" },
            { "authorization_url", oauthUrl},
            { "actual_redirect_uri", InvocationContext.UriInfo.AuthorizationCodeRedirectUri.ToString() },
        };
            
        return QueryHelpers.AddQueryString(bridgeOauthUrl, parameters);
    }
}