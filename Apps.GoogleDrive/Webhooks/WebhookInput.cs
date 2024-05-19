using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Webhooks;

public class WebhookInput
{
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Trigger on item")]
    [StaticDataSource(typeof(ItemTypeDataHandler))]
    public string? ItemType { get; set; }
}