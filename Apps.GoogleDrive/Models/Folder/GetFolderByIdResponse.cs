
using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Folder;

public class GetFolderByIdResponse
{
        [Display("Folder info")]
        public FolderInfo FolderInfo { get; set; } 

        [Display("Parent folder ID")]
        public string? ParentFolderId { get; set; }

        [Display("Found")]
        public bool Found { get; set; }
    
}
