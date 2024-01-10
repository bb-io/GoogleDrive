using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive100.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Webhooks;

public class WebhookInput
{
    [Display("Folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }

    [Display("Trigger on item")]
    [DataSource(typeof(ItemTypeHandler))]
    public string? ItemType { get; set; }
}