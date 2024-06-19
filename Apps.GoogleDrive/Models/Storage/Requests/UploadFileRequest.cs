using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class UploadFileRequest
{
    public FileReference File { get; set; }

    [Display("Parent folder ID")]
    [DataSource(typeof(FolderDataHandler))]
    public string ParentFolderId { get; set; }
}