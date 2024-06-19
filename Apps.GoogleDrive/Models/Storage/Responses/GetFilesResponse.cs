using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class GetFilesResponse
{
    public List<FileReference> Files { get; set; } = new();
}