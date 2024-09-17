using System.Net;
using Apps.GoogleDrive.Clients;
using Apps.GoogleDrive.Webhooks.Handlers;
using Apps.GoogleDrive.Webhooks.Payload;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Webhooks;
using Google.Apis.Drive.v3.Data;
using Google.Apis.DriveActivity.v2.Data;

namespace Apps.GoogleDrive.Webhooks;

[WebhookList]
public class WebhookList : BaseInvocable
{
    public WebhookList(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    //TODO: change to:

    // On item added (with optional parent folder input)
    // On item removed (with optional parent folder input)
    // On item properties updated
    // On item trashed
    // On item untrashed
    // Every event should have an additional optional property to look for only files, folders or both. Dynamic property name: Item type, options: File, Folder, Both
    // See: https://developers.google.com/drive/api/guides/push


    [Webhook("On items added", typeof(ChangesHandler), Description = "On items added")]
    public async Task<WebhookResponse<ChangedItemsPayload>> OnItemsAdded(WebhookRequest webhookRequest, [WebhookParameter(true)] WebhookInput input)
    {
        return await GetAllChanges(webhookRequest, input, "CREATE");
    }

    [Webhook("On items removed", typeof(ChangesHandler), Description = "On items removed")]
    public async Task<WebhookResponse<ChangedItemsPayload>> OnItemsRemoved(WebhookRequest webhookRequest, [WebhookParameter(true)] WebhookInput input)
    {
        return await GetAllChanges(webhookRequest, input, "DELETE");
    }

    [Webhook("On items updated", typeof(ChangesHandler), Description = "On items updated")]
    public async Task<WebhookResponse<ChangedItemsPayload>> OnItemsUpdated(WebhookRequest webhookRequest, [WebhookParameter(true)] WebhookInput input)
    {
        return await GetAllChanges(webhookRequest, input, "EDIT");
    }

    [Webhook("On items restored", typeof(ChangesHandler), Description = "On items restored")]
    public async Task<WebhookResponse<ChangedItemsPayload>> OnItemsRestored(WebhookRequest webhookRequest, [WebhookParameter(true)] WebhookInput input)
    {
        return await GetAllChanges(webhookRequest, input, "RESTORE");
    }

    private async Task<WebhookResponse<ChangedItemsPayload>> GetAllChanges(WebhookRequest webhookRequest, WebhookInput input, string changeType)
    {
        webhookRequest.Headers.TryGetValue("x-goog-resource-state", out var resourceState);
        if (resourceState == "sync")
        {
            webhookRequest.Headers.TryGetValue("x-goog-resource-id", out var resourceId); 
            var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
            await bridgeService.StoreValue(InvocationContext.Bird.Id.ToString() + "_resourceId", resourceId);
            return ReturnPreflight();
        }

        var changes = await DriveFetchChanges();
        if (!changes.Any())
            return ReturnPreflight();

        var specificChanges = await GetSpecificChanges(changes, changeType, input.ItemType ?? "file", input.FolderId);
        if (!specificChanges.Any())
            return ReturnPreflight();

        return new WebhookResponse<ChangedItemsPayload>
        {
            HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
            Result = new ChangedItemsPayload(specificChanges)
        };
    }

    public async Task<List<Change>> DriveFetchChanges()
    {
        var bridgeService = new BridgeService(InvocationContext.UriInfo.BridgeServiceUrl.ToString());
        var savedStartPageToken = (await bridgeService.RetrieveValue(InvocationContext.Bird.Id.ToString())).Replace("\"", "");
        if(!int.TryParse(savedStartPageToken, out var _))
            return new();
        await bridgeService.DeleteValue(InvocationContext.Bird.Id.ToString());
        await Task.Delay(5000);


        var googleDriveService = new GoogleDriveClient(InvocationContext.AuthenticationCredentialsProviders);
        var allChanges = new List<Change>();

        string pageToken = savedStartPageToken;
        while (pageToken != null)
        {
            var request = googleDriveService.Changes.List(pageToken);
            request.Spaces = "drive";
            var changes = request.Execute();
            foreach (var change in changes.Changes)
            {
                allChanges.Add(change);
            }

            if (changes.NewStartPageToken != null)
            {
                savedStartPageToken = changes.NewStartPageToken;
            }
            pageToken = changes.NextPageToken;
        }
        
        await bridgeService.StoreValue(InvocationContext.Bird.Id.ToString(), savedStartPageToken);
        return allChanges;
    }

    private async Task<List<string>> GetSpecificChanges(List<Change> allChanges, string changeType, string itemType, string? folderId)
    {
        var activityClient = new GoogleDriveActivityClient(InvocationContext.AuthenticationCredentialsProviders);

        var filterTime = (DateTimeOffset)(allChanges.OrderBy(x => x.Time).First().Time.Value).AddSeconds(-3);
        string pageToken = null;
        var allItems = new List<DriveItem>();
        do
        {
            var query = new QueryDriveActivityRequest()
            {
                Filter = $"time >= {filterTime.ToUnixTimeMilliseconds()} AND detail.action_detail_case:{changeType}", //CREATE EDIT DELETE MOVE RENAME RESTORE
                PageToken = pageToken
            };
            if (folderId != null)
                query.AncestorName = $"items/{folderId}";

            var request = activityClient.Activity.Query(query);

            var response = await request.ExecuteAsync();
            pageToken = response.NextPageToken;

            var items = response.Activities?
                .Where(x => {
                        switch (changeType)
                        {
                            case "CREATE":
                                return x.PrimaryActionDetail.Create != null;
                            case "EDIT":
                                return x.PrimaryActionDetail.Edit != null;
                            case "DELETE":
                                return x.PrimaryActionDetail.Delete != null;
                            case "MOVE":
                                return x.PrimaryActionDetail.Move != null;
                            case "RENAME":
                                return x.PrimaryActionDetail.Rename != null;
                            case "RESTORE":
                                return x.PrimaryActionDetail.Restore != null;
                        }
                        return true;
                    })
                .Select(x => x.Targets?.FirstOrDefault()?.DriveItem)
                .Where(x => x != null)
                .Where(x =>
                {
                    if (itemType == "folder")
                        return x.MimeType == "application/vnd.google-apps.folder";
                    else if (itemType == "file")
                        return x.MimeType != "application/vnd.google-apps.folder";
                    return true;
                });

            if (items != null)
                allItems.AddRange(items);
        } while (!string.IsNullOrEmpty(pageToken));
        return allItems.Select(x => x.Name.Split("items/").Last()).Where(x => allChanges.Select(y => y.FileId).Contains(x)).Distinct().ToList();
    }

    private WebhookResponse<ChangedItemsPayload> ReturnPreflight()
    {
        return new WebhookResponse<ChangedItemsPayload>
        {
            HttpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK },
            ReceivedWebhookRequestType = WebhookRequestType.Preflight,
            Result = null
        };
    }
}