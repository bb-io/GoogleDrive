using Apps.GoogleDrive.Utils;
using Blackbird.Applications.Sdk.Common.Authentication;
using Google.Apis.Auth.OAuth2;
using Google.Apis.DriveActivity.v2;

namespace Apps.GoogleDrive.Clients;

public class GoogleDriveActivityClient : DriveActivityService
{
    private static Initializer GetInitializer(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        var accessToken = authenticationCredentialsProviders.GetRequiredValue("access_token");
        var credentials = GoogleCredential.FromAccessToken(accessToken);

        return new()
        {
            HttpClientInitializer = credentials,
            ApplicationName = "Blackbird"
        };

    }

    public GoogleDriveActivityClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(GetInitializer(authenticationCredentialsProviders)) { }

}