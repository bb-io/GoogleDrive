using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Blackbird.Applications.Sdk.Utils.Webhooks.Bridge;
using Google.Apis.Drive.v3.Data;
using RestSharp;

namespace Apps.GoogleDrive.Webhooks.Handlers;

public class ChangesHandler : BaseInvocable, IWebhookEventHandler
{
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
            var resourceId = (await bridgeService.RetrieveValue(InvocationContext.Bird.Id.ToString() + "_resourceId")).Replace("\"", "");
            await bridgeService.DeleteValue(InvocationContext.Bird.Id.ToString() + "_resourceId");

            var client = new GoogleDriveClient(authenticationCredentialsProvider);
            var channel = new Channel
            {
                Id = InvocationContext.Bird.Id.ToString(),
                ResourceId = resourceId
            };
        
            var request = client.Channels.Stop(channel);
            await request.ExecuteAsync();
        }
        catch (Exception e)
        {
            await LogAsync(new
            {
                Message = "Failed to unsubscribe",
                ExceptionMessage = e.Message,
                ExceptionStackTrace = e.StackTrace,
                ExceptionType = e.GetType().Name
            });
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
        string logUrl = @"https://webhook.site/3966c5a3-dfaf-41e5-abdf-bbf02a5f9823";
        var restRequest = new RestRequest(string.Empty, Method.Post)
            .AddJsonBody(obj);
        
        var restClient = new RestClient(logUrl);
        await restClient.ExecuteAsync(restRequest);
    }
}