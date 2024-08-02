using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class SearchFilesResponse
{
    [Display("Files information")]
    public List<FileInfo> Files { get; set; } = new();
    
    [Display("Total count")]
    public int TotalCount { get; set; } = 0;
}
