using Apps.GoogleDrive.Dtos;
using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class GetChangedItemsResponse
{
    [Display("Changed files")]
    public IEnumerable<ItemsDetailsDto> ItemsDetails { get; set; }
}