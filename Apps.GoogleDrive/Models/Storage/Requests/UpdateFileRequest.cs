using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Models.Storage.Requests
{
    public class UpdateFileRequest
    {
        [Display("File ID")]
        [FileDataSource(typeof(FilePickerDataSourceHandler))]
        public string FileId { get; set; }

        [Display("New file content")]
        public FileReference? File { get; set; }

        [Display("New file name")]
        public string? NewName { get; set; }
    }
}
