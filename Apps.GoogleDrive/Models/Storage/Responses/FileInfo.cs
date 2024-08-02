using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class FileInfo
{
    [Display("File ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("File name")]
    public string FileName { get; set; } = string.Empty;
    
    [Display("File size")]
    public long Size { get; set; } = 0;
    
    [Display("Mime type")]
    public string MimeType { get; set; } = string.Empty;
}