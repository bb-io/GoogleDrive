using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class CreateFolderRequest
{
    [Display("Folder name")]
    public string FolderName { get; set; }

    [Display("Parent folder")]
    [DataSource(typeof(FolderDataHandler))]
    public string ParentFolderId { get; set; }
}