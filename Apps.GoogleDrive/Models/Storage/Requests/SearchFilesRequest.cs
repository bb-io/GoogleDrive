using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class SearchFilesRequest : FindFileRequest
{
    [Display("Limit", Description = "The maximum number of files to return. Default is 50. Maximum is 100.")]
    public int? Limit { get; set; } = 50;
}