using Apps.GoogleDrive.Clients;
using Apps.GoogleDrive.Dtos;
using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Utils.Extensions.Files;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Drive.v3.Data;
using Google.Apis.DriveActivity.v2.Data;
using System.Net.Mime;
using static Google.Apis.Requests.BatchRequest;

namespace Apps.GoogleDrive.Actions;

[ActionList]
public class StorageActions : DriveInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public StorageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    #region File actions

    private Dictionary<string, string> _mimeMap = new Dictionary<string, string>
    {
        { "application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { "application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { "application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
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
    public async Task<FileReference> GetFile([ActionParameter] GetFileRequest input)
    {
        var request = Client.Files.Get(input.FileId);
        request.SupportsAllDrives = true;
        var fileMetadata = request.Execute();

        byte[] data;
        var fileName = fileMetadata.Name;
        using (var stream = new MemoryStream())
        {
            if (fileMetadata.MimeType.Contains("vnd.google-apps"))
            {
                if (!_mimeMap.ContainsKey(fileMetadata.MimeType))
                    throw new Exception($"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");
                var exportRequest = Client.Files.Export(input.FileId, _mimeMap[fileMetadata.MimeType]);
                exportRequest.DownloadWithStatus(stream).ThrowOnFailure();
                fileName += _extensionMap[fileMetadata.MimeType];
            }
            else                    
                request.DownloadWithStatus(stream).ThrowOnFailure();

            data = stream.ToArray();
        }
        using var stream2 = new MemoryStream(data);
        var file = await _fileManagementClient.UploadAsync(stream2, fileMetadata.MimeType, fileName);
        return file;
    }

    [Action("Upload file", Description = "Upload a file")]
    public void UploadFile([ActionParameter] UploadFileRequest input)
    {
        var body = new Google.Apis.Drive.v3.Data.File();
        body.Name = input.File.Name;
        body.Parents = new List<string> { input.ParentFolderId };

        using var fileBytes = _fileManagementClient.DownloadAsync(input.File).Result;
        var request = Client.Files.Create(body, fileBytes, null);
        request.Upload();    
    }

    [Action("Delete item", Description = "Delete item (file/folder)")]
    public void DeleteItem([ActionParameter] DeleteItemRequest input)
    {
        var request = Client.Files.Delete(input.ItemId);
        request.SupportsAllDrives = true;
        request.Execute();
    }

    #endregion

    #region Folder actions

    [Action("Create folder", Description = "Create folder")]
    public CreateFolderResponse CreateFolder([ActionParameter] CreateFolderRequest input)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.FolderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> { input.ParentFolderId }
        };
        var request = Client.Files.Create(fileMetadata);
        request.SupportsAllDrives = true;
        var response = request.Execute();
        return new CreateFolderResponse
        { 
            FolderID = response.Id,
            FolderName = response.Name
        };
    }

    #endregion

    #region Labels actions

    [Action("Add labels to item", Description = "Add labels to item (file/folder)")]
    public async Task AddLabelsToItem(
        [ActionParameter] DeleteItemRequest itemRequest, 
        [ActionParameter] GetLabelRequest labelsRequest)
    {
        var request = Client.Files.ModifyLabels(
            new ModifyLabelsRequest()
            {
                LabelModifications = new List<LabelModification>() { 
                    new LabelModification() { 
                        LabelId = labelsRequest.LabelId, 
                        FieldModifications = new List<LabelFieldModification>() { 
                            new LabelFieldModification() { } 
                        } 
                    } 
                }
            }, itemRequest.ItemId);
        await request.ExecuteAsync();
    }

    #endregion
}