using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Google.Apis.Drive.v3.Data;

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
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        
        string value;
        try
        {
            value = await bridgeService.RetrieveValue(InvocationContext.Bird.Id.ToString() + "_resourceId");
        }
        catch (Exception e)
        {
            // If resource id is not found, there is no need to unsubscribe
            return;
        }
        
        var resourceId = value.Replace("\"", "");
        if (resourceId.Contains(StoredValueNotFound) || string.IsNullOrEmpty(resourceId))
        {
            // If resource id is not found, there is no need to unsubscribe
            return;
        }
        
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

    [Period(10000)]
    public async Task RenewSubscription(IEnumerable<AuthenticationCredentialsProvider> creds,
        Dictionary<string, string> values)
    {
        await UnsubscribeAsync(creds, values);
        await SubscribeAsync(creds, values);
    }
}