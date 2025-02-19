using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class UploadFilesRequest
{
    public FileReference File { get; set; }

    [Display("Parent folder ID")]
    [DataSource(typeof(FolderDataHandler))]
    public string ParentFolderId { get; set; }

    [Display("Save as")]
    [DataSource(typeof(MimeTypeDataHandler))]
    public string? SaveAs { get; set; }
}