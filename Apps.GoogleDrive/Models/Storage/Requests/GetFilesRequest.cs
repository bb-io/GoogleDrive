using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Storage.Requests;

public class GetFilesRequest
{
    [Display("File IDs")]
    [DataSource(typeof(FileDataHandler))]
    public IEnumerable<string> FileIds { get; set; }
}