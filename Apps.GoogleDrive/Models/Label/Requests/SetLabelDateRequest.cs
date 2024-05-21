using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelDateRequest : SetLabelFieldBaseRequest
    {
        [Display("Date")]
        public DateTime DateFieldValue { get; set; }
    }
}
