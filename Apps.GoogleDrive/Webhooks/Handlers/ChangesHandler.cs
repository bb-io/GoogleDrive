using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Google.Apis.Drive.v3.Data;
using RestSharp;

namespace Apps.GoogleDrive.Webhooks.Handlers;

public class ChangesHandler : BaseInvocable, IWebhookEventHandler
{
    private const string StoredValueNotFound = "Stored value was not found";
    
    public ChangesHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    private Channel BuildChannel(Dictionary<string, string> values)
    {
        return new Channel
        {
            Payload = true,
            Id = InvocationContext.Bird.Id.ToString(),
            Expiration = new DateTimeOffset(DateTime.Now.AddDays(7)).ToUnixTimeMilliseconds(),
            Type = "web_hook",
            Address = values["payloadUrl"]
        };
    }

    public async Task SubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, Dictionary<string, string> values)
    {
        var client = new GoogleDriveClient(authenticationCredentialsProvider);
        var channel = BuildChannel(values);

        var stateToken = client.Changes.GetStartPageToken().Execute();
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        await bridgeService.StoreValue(InvocationContext.Bird.Id.ToString(), stateToken.StartPageTokenValue);
        
        var request = client.Changes.Watch(channel, stateToken.StartPageTokenValue);
        await request.ExecuteAsync();
    }

    public async Task UnsubscribeAsync(IEnumerable<AuthenticationCredentialsProvider> authenticationCredentialsProvider, Dictionary<string, string> values)
    {
        try
        {
            var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());

            string value = await bridgeService.RetrieveValue(InvocationContext.Bird.Id.ToString() + "_resourceId");
            var resourceId = value.Replace("\"", "");

            await LogAsync(new
            {
                Status = "After retrieving value",
                BirdId = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            });

            if (resourceId.Contains(StoredValueNotFound) || string.IsNullOrEmpty(resourceId))
            {
                // If resource id is not found, there is no need to unsubscribe
                return;
            }

            await bridgeService.DeleteValue(InvocationContext.Bird.Id.ToString() + "_resourceId");

            await LogAsync(new
            {
                Status = "After deleting value",
                BirdId = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            });

            var client = new GoogleDriveClient(authenticationCredentialsProvider);
            var channel = new Channel
            {
                Id = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            };

            await LogAsync(new
            {
                Status = "Before stopping channel",
                BirdId = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            });

            var request = client.Channels.Stop(channel);

            await LogAsync(new
            {
                Status = "After stopping channel",
                BirdId = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            });

            await request.ExecuteAsync();

            await LogAsync(new
            {
                Status = "After executing request",
                BirdId = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            });
        }
        catch (Exception e)
        {
            await LogAsync(new
            {
                Status = "Error",
                BirdId = InvocationContext.Bird.Id.ToString(),
                Message = e.Message,
                StackTrace = e.StackTrace,
                InnerException = e.InnerException?.Message
            });
            
            throw;
        }
    }

    [Period(10000)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> creds,
        Dictionary<string, string> values)
    {
        await UnsubscribeAsync(creds, values);
        await SubscribeAsync(creds, values);
    }

    private async Task LogAsync<T>(T obj)
        where T : class
    {
        var logUrl = @"https://webhook.site/3966c5a3-dfaf-41e5-abdf-bbf02a5f9823";

        var restRequest = new RestRequest(string.Empty, Method.Post)
            .AddJsonBody(obj);
        
        var restClient = new RestClient(logUrl);
        await restClient.ExecuteAsync(restRequest);
    }
}