using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Folder
{
    public class FolderInfo
    {
        [Display("Folder ID")]
        public string Id { get; set; }

        [Display("Folder name")]
        public string Name { get; set; }

        [Display("Parent folder ID")]
        public string ParentFolderId { get; set; }

        [Display("Web view link")]
        public string WebViewLink { get; set; }

        [Display("Created time")]
        public DateTime? CreatedTime { get; set; }

        [Display("Update time")]
        public DateTime? ModifiedTime { get; set; }

        [Display("Size")]
        public long? Size { get; set; }

    }
}