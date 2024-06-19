using Blackbird.Applications.Sdk.Common.Authentication;
using Google.Apis.Auth.OAuth2;
using Google.Apis.DriveLabels.v2;
using Google.Apis.Services;

namespace Apps.GoogleDrive.Clients
{
    public class GoogleDriveLabelClient : DriveLabelsService
    {
        private static Initializer GetInitializer(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders)
        {
            var accessToken = authenticationCredentialsProviders.First(p => p.KeyName == "access_token").Value;
            GoogleCredential credentials = GoogleCredential.FromAccessToken(accessToken);

            return new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = "Blackbird"
            };

        }

        public GoogleDriveLabelClient(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProviders) : base(GetInitializer(authenticationCredentialsProviders)) { }
    }
}
