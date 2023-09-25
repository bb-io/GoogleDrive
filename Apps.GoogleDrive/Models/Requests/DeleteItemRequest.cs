﻿using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Requests;

public class DeleteItemRequest
{
    [Display("Item")]
    [DataSource(typeof(DriveItemDataHandler))]
    public string ItemId { get; set; }
}