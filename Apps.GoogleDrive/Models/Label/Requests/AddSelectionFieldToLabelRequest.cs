using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class AddSelectionFieldToLabelRequest
    {
        [Display("Display name")]
        public string DisplayName { get; set; }

        [Display("Choices")]
        public List<string> Choices { get; set; }
    }
}
