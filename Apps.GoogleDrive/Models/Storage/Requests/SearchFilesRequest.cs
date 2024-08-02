using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class SearchFilesRequest
{
    [Display("File name")]
    public string? FileName { get; set; }
}