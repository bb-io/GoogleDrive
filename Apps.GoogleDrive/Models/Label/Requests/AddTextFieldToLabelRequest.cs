using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class AddTextFieldToLabelRequest
    {
        [Display("Display name")]
        public string DisplayName { get; set; }
    }
}
