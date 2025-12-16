using Apps.GoogleDrive.DataSourceHandler;
using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.Polling.Models
{
    public class OnFileCreatedRequest
    {
        [Display("Folder ID")]
        [FileDataSource(typeof(FolderPickerDataSourceHandler))]
        public string FolderId { get; set; }

        [Display("File name contains", Description = "Return only files where name contains this text")]
        public string? FileNameContains { get; set; }

        [Display("Mime type", Description = "Return only files with this exact mime type (e.g. application/pdf)")]
        [StaticDataSource(typeof(MimeTypeDataHandler))]
        public string? MimeType { get; set; }
    }
}
