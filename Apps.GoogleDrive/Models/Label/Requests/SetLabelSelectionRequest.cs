using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelSelectionRequest
    {
        [Display("Selection")]
        [DataSource(typeof(SelectionFieldDataHandler))]
        public string SelectionFieldValue { get; set; }
    }
}
