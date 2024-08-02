using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class SearchFilesResponse
{
    [Display("Files")]
    public List<FileDto> Files { get; set; } = new();
    
    [Display("Total count")]
    public int TotalCount { get; set; } = 0;
}

public class FileDto
{
    [Display("File ID")]
    public string Id { get; set; } = string.Empty;
    
    [Display("File name")]
    public string FileName { get; set; } = string.Empty;
    
    [Display("File size")]
    public long Size { get; set; } = 0;
}