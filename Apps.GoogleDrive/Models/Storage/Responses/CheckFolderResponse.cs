using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses
{
    public class CheckFolderResponse
    {
        [Display("Exists")]
        public bool Exists { get; set; }

        [Display("Folder ID")]
        public string FolderId { get; set; }
    }
}
