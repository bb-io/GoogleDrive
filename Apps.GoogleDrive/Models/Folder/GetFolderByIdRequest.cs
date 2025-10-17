using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Folder
{
    public class GetFolderByIdRequest
    {
        [Display("Folder ID"), DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }
    }
}
