using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelTextRequest : SetLabelFieldBaseRequest
    {
        [Display("Text")]
        public string TextFieldValue { get; set; }
    }
}
