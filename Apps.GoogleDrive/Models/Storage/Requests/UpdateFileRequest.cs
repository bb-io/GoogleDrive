using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.GoogleDrive.Models.Storage.Requests
{
    public class UpdateFileRequest
    {
        [Display("File ID")]
        [DataSource(typeof(FileDataHandler))]
        public string FileId { get; set; }

        [Display("New file content")]
        public FileReference? File { get; set; }

        [Display("New file name")]
        public string? NewName { get; set; }
    }
}
