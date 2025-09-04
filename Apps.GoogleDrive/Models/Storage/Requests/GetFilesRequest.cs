using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class DownloadFileRequest
{
    [Display("File ID")]
    [DataSource(typeof(FileDataHandler))]
    public string FileId { get; set; }
}