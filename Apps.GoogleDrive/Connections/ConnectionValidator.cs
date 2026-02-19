using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Connections;
using Blackbird.Applications.Sdk.Common.Exceptions;
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

            var aboutRequest = client.About.Get();
            aboutRequest.Fields = "user/me";
            await aboutRequest.ExecuteAsync(cancellationToken);  
        }
        catch (GoogleApiException gae) when (gae.HttpStatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
        {
            return new()
            {
                IsValid = false,
                Message = $"{gae.Message} - {gae.InnerException}"
            };
        }
        catch (Exception ex)
        {
            return new()
            {
                IsValid = true
            };
        }

        return new() { IsValid = true };
    }
}