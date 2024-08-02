using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class FindFileRequest
{
    [Display("Folder ID"), DataSource(typeof(FolderDataHandler))]
    public string? FolderId { get; set; }
    
    [Display("File name")]
    public string? FileName { get; set; }
    
    [Display("Mime type"), StaticDataSource(typeof(MimeTypeDataHandler))]
    public string? MimeType { get; set; }
}