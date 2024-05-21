using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelFieldBaseRequest
    {
        [Display("Field ID")]
        [DataSource(typeof(LabelTypedFieldDataHandler))]
        public string FieldId { get; set; }
    }
}
