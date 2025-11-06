using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Storage.Requests
{
    public class CheckFolderRequest
    {
        [Display("Folder name")]
        public string FolderName { get; set; }

        [Display("Parent folder")]
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string ParentFolderId { get; set; }
    }
}
