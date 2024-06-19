using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses
{
    public class CreateFolderResponse
    {
        [Display("Folder ID")]
        public string FolderID { get; set; }

        [Display("Folder Name")]
        public string FolderName { get; set; }
    }
}
