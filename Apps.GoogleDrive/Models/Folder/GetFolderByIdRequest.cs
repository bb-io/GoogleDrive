using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Folder
{
    public class GetFolderByIdRequest
    {
        [Display("Folder ID")] 
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string FolderId { get; set; }
    }
}
