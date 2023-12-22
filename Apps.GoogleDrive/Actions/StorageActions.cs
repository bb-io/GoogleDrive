using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Google.Apis.Download;
using System.Net.Mime;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.GoogleDrive.Actions;

[ActionList]
public class StorageActions : DriveInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public StorageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(
        invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    #region File actions

    //[Action("Get all items details", Description = "Get all items(files/folders) details")]
    //public GetAllItemsResponse GetAllItemsDetails()
    //{
    //    var filesListr = Client.Files.List();
    //    filesListr.SupportsAllDrives = true;
    //    var filesList = filesListr.Execute();

    //    var filesDetails = new List<ItemsDetailsDto>();
    //    foreach (var file in filesList.Files)
    //    {
    //        filesDetails.Add(new ItemsDetailsDto
    //        {
    //            Id = file.Id,
    //            Name = file.Name,
    //            MimeType = file.MimeType
    //        });
    //    }

    //    return new GetAllItemsResponse(filesDetails);
    //}

    //[Action("Get changed files", Description = "Get all files that have been created or modified in the last time period")]
    //public async Task<GetChangedItemsResponse> GetChangedFiles([ActionParameter] GetChangedFilesRequest input)
    //{
    //    var activityClient = new GoogleDriveActivityClient(InvocationContext.AuthenticationCredentialsProviders);
    //    var driveItems = new List<DriveItem>();
    //    var deletedItemIds = new List<string>();

    //    string? pageToken = null;
    //    var filterTime = (DateTimeOffset)(DateTime.Now - TimeSpan.FromHours(input.LastHours));

    //    do
    //    {
    //        var request = activityClient.Activity.Query(new()
    //        {
    //            Filter = $"time >= {filterTime.ToUnixTimeMilliseconds()} AND detail.action_detail_case:(CREATE EDIT DELETE)",
    //            PageToken = pageToken
    //        });

    //        var response = await request.ExecuteAsync();
    //        pageToken = response.NextPageToken;

    //        var deletedItems = response.Activities?
    //            .Where(x => x.PrimaryActionDetail.Delete != null)
    //            .Select(x => x.Targets?.FirstOrDefault()?.DriveItem)
    //            .Where(x => x != null)
    //            .Where(x => x.MimeType != "application/vnd.google-apps.folder")
    //            .Select(x => x.Name);

    //        var items = response.Activities?
    //            .Where(x => x.PrimaryActionDetail.Create != null || x.PrimaryActionDetail.Edit != null)
    //            .Select(x => x.Targets?.FirstOrDefault()?.DriveItem)
    //            .Where(x => x != null)
    //            .Where(x => x.MimeType != "application/vnd.google-apps.folder");

    //        if (items != null)
    //            driveItems.AddRange(items);

    //        if (deletedItems != null)
    //            deletedItemIds.AddRange(deletedItems);
    //    } while (!string.IsNullOrEmpty(pageToken));

    //    var allChangedItems = driveItems.Where(x => !deletedItemIds.Contains(x.Name)).DistinctBy(x => x.Name);

    //    return new GetChangedItemsResponse
    //    {
    //        ItemsDetails = allChangedItems.Select(x => new ItemsDetailsDto
    //        {
    //            Name = x.Title,
    //            Id = x.Name.Split("/").Last(),
    //            MimeType = x.MimeType,
    //        })
    //    };
    //}

    private Dictionary<string, string> _mimeMap = new Dictionary<string, string>
    {
        {
            "application/vnd.google-apps.document",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        },
        {
            "application/vnd.google-apps.presentation",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        },
        {
            "application/vnd.google-apps.spreadsheet",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        },
        { "application/vnd.google-apps.drawing", "application/pdf" }
    };

    private Dictionary<string, string> _extensionMap = new Dictionary<string, string>
    {
        { "application/vnd.google-apps.document", ".docx" },
        { "application/vnd.google-apps.presentation", ".pptx" },
        { "application/vnd.google-apps.spreadsheet", ".xlsx" },
        { "application/vnd.google-apps.drawing", ".pdf" }
    };

    [Action("Download file", Description = "Download a file")]
    public Task<FileReference> GetFile([ActionParameter] GetFileRequest input)
    {
        var request = Client.Files.Get(input.FileId);
        var fileMetadata = request.Execute();

        byte[] data;
        var fileName = fileMetadata.Name;
        using var stream = new MemoryStream();
        if (fileMetadata.MimeType.Contains("vnd.google-apps"))
        {
            if (!_mimeMap.ContainsKey(fileMetadata.MimeType))
                throw new Exception(
                    $"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");
            var exportRequest = Client.Files.Export(input.FileId, _mimeMap[fileMetadata.MimeType]);
            exportRequest.DownloadWithStatus(stream).ThrowOnFailure();
            fileName += _extensionMap[fileMetadata.MimeType];
        }
        else
            request.DownloadWithStatus(stream).ThrowOnFailure();

        data = stream.ToArray();

        return _fileManagementClient.UploadAsync(stream, MediaTypeNames.Application.Octet, fileName);
    }

    [Action("Upload file", Description = "Upload a file")]
    public async Task UploadFile([ActionParameter] UploadFileRequest input)
    {
        var body = new Google.Apis.Drive.v3.Data.File();
        body.Name = input.File.Name;
        body.Parents = new List<string> { input.ParentFolderId };

        var stream = await _fileManagementClient.DownloadAsync(input.File);

        var request = Client.Files.Create(body, stream, null);
        await request.UploadAsync();
    }

    [Action("Delete item", Description = "Delete item (file/folder)")]
    public void DeleteItem([ActionParameter] DeleteItemRequest input)
    {
        Client.Files.Delete(input.ItemId).Execute();
    }

    #endregion

    #region Folder actions

    [Action("Create folder", Description = "Create folder")]
    public void CreateFolder([ActionParameter] CreateFolderRequest input)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.FolderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> { input.ParentFolderId }
        };
        var request = Client.Files.Create(fileMetadata);
        request.Execute();
    }

    #endregion
}