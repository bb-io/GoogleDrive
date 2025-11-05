using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class DownloadFileRequest
{
    [Display("File ID")]
    [FileDataSource(typeof(FilePickerDataSourceHandler))]
    public string FileId { get; set; }
}