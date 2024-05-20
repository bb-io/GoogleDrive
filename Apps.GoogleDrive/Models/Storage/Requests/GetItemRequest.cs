using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class GetItemRequest
{
    [Display("Item ID")]
    [DataSource(typeof(DriveItemDataHandler))]
    public string ItemId { get; set; }
}