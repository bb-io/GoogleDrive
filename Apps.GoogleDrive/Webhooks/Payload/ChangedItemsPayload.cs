using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Webhooks.Payload;

public class ChangedItemsPayload
{
    public ChangedItemsPayload(List<string> itemIds)
    {
        ItemIds = itemIds;
    }

    [Display("Items IDs")]
    public List<string> ItemIds { get; set; }
}