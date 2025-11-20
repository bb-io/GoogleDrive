using Blackbird.Applications.Sdk.Common;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class UploadFileResponse
{
    [Display("File ID")]
    public string Id { get; set; } = string.Empty;
}
