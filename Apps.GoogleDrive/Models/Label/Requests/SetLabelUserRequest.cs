using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class SetLabelUserRequest : SetLabelFieldBaseRequest
    {
        [Display("User", Description = "User's email")]
        public string UserFieldValue { get; set; }
    }
}
