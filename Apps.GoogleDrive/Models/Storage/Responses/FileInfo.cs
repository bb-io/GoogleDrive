using Blackbird.Applications.Sdk.Common;
using File = Google.Apis.Drive.v3.Data.File;

namespace Apps.GoogleDrive.Models.Storage.Responses;

public class FileInfo
{
    [Display("File ID")] public string Id { get; set; } = string.Empty;

    [Display("File name")] public string FileName { get; set; } = string.Empty;

    [Display("File size")] public long Size { get; set; } = 0;

    [Display("Mime type")] public string MimeType { get; set; } = string.Empty;

    [Display("Created at")] public DateTime? CreatedAt { get; set; }

    [Display("Updated at")] public DateTime? UpdatedAt { get; set; }

    [Display("Parent folder ID")] public string? ParentID { get; set; }

    [Display("Owner name")] public string? OwnerName { get; set; }
    [Display("Owner email")] public string? OwnerEmail { get; set; }

    [Display("Last modifier name")] public string? LastModifierName { get; set; }
    [Display("Last modifier email")] public string? LastModifierEmail { get; set; }

    public FileInfo()
    {
    }

    public FileInfo(File file)
    {
        Id = file.Id;
        FileName = file.Name;
        Size = file.Size ?? 0;
        MimeType = file.MimeType;
        CreatedAt = file.CreatedTimeDateTimeOffset?.DateTime;
        UpdatedAt = file.ModifiedTimeDateTimeOffset?.DateTime;
        ParentID = file.Parents?.FirstOrDefault() ?? "root";
        var primaryOwner = file.Owners?.FirstOrDefault();
        OwnerName = primaryOwner?.DisplayName;
        OwnerEmail = primaryOwner?.EmailAddress;
        LastModifierName = file.LastModifyingUser?.DisplayName;
        LastModifierEmail = file.LastModifyingUser?.EmailAddress;
    }
}