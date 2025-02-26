using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Polling.Models
{
    public class OnFileCreatedRequest
    {
        [Display("Folder ID")]
        [DataSource(typeof(FolderDataHandler))]
        public string FolderId { get; set; }
    }
}
