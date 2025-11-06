using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class FindFileRequest
{
    [Display("Folder ID")]
    [FileDataSource(typeof(FolderPickerDataSourceHandler))]
    public string? FolderId { get; set; }
    
    [Display("File name")]
    public string? FileName { get; set; }
    
    [Display("Mime type"), StaticDataSource(typeof(MimeTypeDataHandler))]
    public string? MimeType { get; set; }

    [Display("File name must be exact match?")]
    public bool? FileExactMatch { get; set; }
}