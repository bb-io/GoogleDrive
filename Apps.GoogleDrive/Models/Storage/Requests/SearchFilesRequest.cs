using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class SearchFilesRequest
{
    [Display("Folder ID")]
    [FileDataSource(typeof(FolderPickerDataSourceHandler))]
    public string? FolderId { get; set; }

    [Display("Include subfolders?")]
    public bool? IncludeSubfolders { get; set; }

    [Display("Max subfolder level to search")]
    public double? MaxSubfolderLevel { get; set; }

    [Display("File name")]
    public string? FileName { get; set; }

    [Display("File name must be exact match?")]
    public bool? FileExactMatch { get; set; }

    [Display("Mime type"), StaticDataSource(typeof(MimeTypeDataHandler))]
    public string? MimeType { get; set; }

    [Display("Limit", Description = "The maximum number of files to return. Default is 50. Maximum is 100.")]
    public int? Limit { get; set; } = 50;
}