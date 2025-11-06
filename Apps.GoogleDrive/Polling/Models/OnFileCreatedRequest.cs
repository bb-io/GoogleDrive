using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Polling.Models
{
    public class OnFileCreatedRequest
    {
        [Display("Folder ID")]
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string FolderId { get; set; }
    }
}
