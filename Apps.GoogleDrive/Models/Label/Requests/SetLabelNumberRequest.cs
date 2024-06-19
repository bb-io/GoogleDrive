using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelNumberRequest : SetLabelFieldBaseRequest
    {
        [Display("Number")]
        public int NumberFieldValue { get; set; }
    }
}
