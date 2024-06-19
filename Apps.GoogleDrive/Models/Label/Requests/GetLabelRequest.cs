using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class GetLabelRequest
    {
        [Display("Label ID")]
        [DataSource(typeof(LabelDataHandler))]
        public string LabelId { get; set; }
    }
}
