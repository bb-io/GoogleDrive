using Blackbird.Applications.Sdk.Common.Authentication;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace Apps.GoogleDrive.Clients;

public class GoogleDriveClient : DriveService
{
    private static Initializer GetInitializer(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
    {
        //var serviceAccountConfString = authenticationCredentialsProviders.First(p => p.KeyName == "serviceAccountConfString").Value;
        //string[] scopes = { DriveService.Scope.Drive };
        //ServiceAccountCredential? credential = GoogleCredential.FromJson(serviceAccountConfString)
        //                                      .CreateScoped(scopes)
        //                                      .UnderlyingCredential as ServiceAccountCredential;
        var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "access_token").Value;
        GoogleCredential credentials = GoogleCredential.FromAccessToken(accessToken);

        return new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "Blackbird"
        };
            
    }

    public GoogleDriveClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base (GetInitializer(authenticationCredentialsProviders)) { }
}