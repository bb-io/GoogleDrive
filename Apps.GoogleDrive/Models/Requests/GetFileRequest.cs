﻿using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.Models.Requests
{
    public class GetFileRequest
    {
        [Display("File ID")]
        public string FileId { get; set; }
    }
}
