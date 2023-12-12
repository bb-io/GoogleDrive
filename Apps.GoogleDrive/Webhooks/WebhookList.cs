using System.Net;
using Apps.GoogleDrive.Webhooks.Handlers.UserHandlers;
using Apps.GoogleDrive.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common.Webhooks;

namespace Apps.GoogleDrive.Webhooks;

[WebhookList]
public class WebhookList
{
    [Webhook("On folder content added", typeof(FolderContentAddedHandler), Description = "On folder content added")]
    public async Task<WebhookResponse<FolderContentChangedPayload>> FolderContentAdded(WebhookRequest webhookRequest) // This output value is not great and contains useless information
    {
        webhookRequest.Headers.TryGetValue("X-Goog-Channel-Token", out var stateToken);
        webhookRequest.Headers.TryGetValue("X-Goog-Resource-State", out var resourceState);
        webhookRequest.Headers.TryGetValue("X-Goog-Channel-ID", out var channelId);
        webhookRequest.Headers.TryGetValue("X-Goog-Resource-ID", out var resourceId); // This is the watched resource (in our case still the folder)
        
        if (resourceState != "update")
        {
            return new WebhookResponse<FolderContentChangedPayload>
            {
                HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
                Result = null
            };
        }

        return new WebhookResponse<FolderContentChangedPayload>
        {
            HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
            Result = new FolderContentChangedPayload()
            {
                StateToken = stateToken,
                ResourceState = resourceState, //update, trash
                ChannelId = channelId,
                ResourceId = resourceId
            }
        };
    }

    [Webhook("On folder content removed", typeof(FolderContentAddedHandler), Description = "On folder content removed")]
    public async Task<WebhookResponse<FolderContentChangedPayload>> FolderContentRemoved(WebhookRequest webhookRequest)
    {
        webhookRequest.Headers.TryGetValue("X-Goog-Channel-Token", out var stateToken);
        webhookRequest.Headers.TryGetValue("X-Goog-Resource-State", out var resourceState);
        webhookRequest.Headers.TryGetValue("X-Goog-Channel-ID", out var channelId);
        webhookRequest.Headers.TryGetValue("X-Goog-Resource-ID", out var resourceId);
        if (resourceState != "trash")
        {
            return new WebhookResponse<FolderContentChangedPayload>
            {
                HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
                ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            };
        }

        return new WebhookResponse<FolderContentChangedPayload>
        {
            HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
            Result = new FolderContentChangedPayload()
            {
                StateToken = stateToken,
                ResourceState = resourceState, //update, trash
                ChannelId = channelId,
                ResourceId = resourceId
            }
        };
    }
}