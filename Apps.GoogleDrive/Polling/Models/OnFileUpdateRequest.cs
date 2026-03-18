using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Polling.Models;

public class OnFileUpdateRequest
{
    [Display("Folder ID")]
    [FileDataSource(typeof(FolderPickerDataSourceHandler))]
    public string? FolderId { get; set; }

    [Display("Include subfolders?")]
    public bool? IncludeSubfolders { get; set; }

    [Display("Max subfolder level to search")]
    public double? MaxSubfolderLevel { get; set; }

    [Display("File ID")]
    [DataSource(typeof(FileDataHandler))]
    public string? FileId { get; set; }
}
