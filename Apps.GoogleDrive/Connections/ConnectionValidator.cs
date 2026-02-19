using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Google;

namespace Apps.GoogleDrive.Connections;

public class ConnectionValidator : IConnectionValidator
{
    public async ValueTask<ConnectionValidationResponse> ValidateConnection(
        IEnumerable<AuthenticationCredentialsProvider> authProviders, CancellationToken cancellationToken)
    {
        try
        {
            var client = new GoogleDriveClient(authProviders);

            var request = client.Files.List();
            request.SupportsAllDrives = true;

            await request.ExecuteAsync(cancellationToken);

            return new() { IsValid = true };
        }
        catch (Exception ex)
        {
            var message = ex switch
            {
                GoogleApiException gae when (int)gae.HttpStatusCode == 401 || (int)gae.HttpStatusCode == 403
                    => "Google Drive connection is not authorized or has expired. Please reconnect the connection.",
                _ => $"Connection validation failed: {ex.Message}"
            };

            return new()
            {
                IsValid = false,
                Message = message
            };
        }
    }
}