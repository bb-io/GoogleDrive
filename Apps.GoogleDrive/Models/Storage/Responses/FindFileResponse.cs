using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class FindFileResponse
{
    [Display("File information")]
    public FileInfo FileInfo { get; set; } = new();
    
    [Display("Is found")]
    public bool Found { get; set; }
}