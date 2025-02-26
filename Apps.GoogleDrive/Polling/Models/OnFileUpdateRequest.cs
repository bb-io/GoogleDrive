using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Polling.Models
{
    public class OnFileUpdateRequest
    {
        [Display("Folder ID")]
        [DataSource(typeof(FolderDataHandler))]
        public string? FolderId { get; set; }

        [Display("File ID")]
        [DataSource(typeof(FileDataHandler))]
        public string? FileId { get; set; }
    }
}
