using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Folder
{
    public class GetFolderByIdRequest
    {
        [Display("Folder ID")]
        public string FolderId { get; set; } = string.Empty;
    }
}
