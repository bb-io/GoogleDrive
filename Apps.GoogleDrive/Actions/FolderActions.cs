using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models;
using Apps.GoogleDrive.Models.Folder;
using Apps.GoogleDrive.Models.Label.Responses;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using FileInfo = Apps.GoogleDrive.Models.Storage.Responses.FileInfo;

namespace Apps.GoogleDrive.Actions;

[ActionList("Folders")]
public class FolderActions : DriveInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public FolderActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

    [Action("Create folder", Description = "Create folder")]
    public CreateFolderResponse CreateFolder([ActionParameter] CreateFolderRequest input)
    {
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.FolderName,
            MimeType = "application/vnd.google-apps.folder",
            Parents = new List<string> { input.ParentFolderId }
        };
        var request = ExecuteWithErrorHandling(() => Client.Files.Create(fileMetadata));
        request.SupportsAllDrives = true;
        var response = ExecuteWithErrorHandlingAsync(() => request.ExecuteAsync())
                     .GetAwaiter()
                     .GetResult();
        return new CreateFolderResponse
        {
            FolderID = response.Id,
            FolderName = response.Name
        };
    }

    [Action("Get folder information", Description = "Get folder information, including its parent folder ID, using the folder ID")]
    public async Task<GetFolderByIdResponse> GetFolderById([ActionParameter] GetFolderByIdRequest input)
    {
        var request = ExecuteWithErrorHandling(() => Client.Files.Get(input.FolderId));
        request.SupportsAllDrives = true;
        request.Fields = "id, name, mimeType, parents, webViewLink, createdTime, modifiedTime, size";
        var folder = ExecuteWithErrorHandlingAsync(() => request.ExecuteAsync())
                     .GetAwaiter()
                     .GetResult();
        
        if (folder.MimeType != "application/vnd.google-apps.folder")
            throw new PluginMisconfigurationException($"The provided ID ({input.FolderId}) does not correspond to a folder.");

        string? parentFolderId = folder.Parents?.FirstOrDefault();

        var mapped = new Apps.GoogleDrive.Models.Folder.FolderInfo
        {
            Id = folder.Id,
            Name = folder.Name,
            ParentFolderId = parentFolderId,
            WebViewLink = folder.WebViewLink,
            CreatedTime = folder.CreatedTime,
            ModifiedTime = folder.ModifiedTime,
            Size = folder.Size
        };

        return new()
        {
            FolderInfo = mapped,
            Found = true
        };

        
    }

    [Action("Check folder exists", Description = "Given a folder name and a parent folder, checks if a folder with the same name exists")]
    public async Task<CheckFolderResponse> CheckFolderExists([ActionParameter] CheckFolderRequest input)
    {
        string query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{input.FolderName}' and '{input.ParentFolderId}' in parents and trashed = false";

        var listRequest = Client.Files.List();
        listRequest.Q = query;
        listRequest.Fields = "files(id, name)";
        listRequest.SupportsAllDrives = true;
        listRequest.IncludeItemsFromAllDrives = true;

        var response = await ExecuteWithErrorHandlingAsync(async () => await listRequest.ExecuteAsync());

        if (response.Files != null && response.Files.Any())
        {
            return new CheckFolderResponse
            {
                Exists = true,
                FolderId = response.Files.First().Id
            };
        }

        return new CheckFolderResponse
        {
            Exists = false,
            FolderId = null
        };
    }
}