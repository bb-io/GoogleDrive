using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Upload;
using FileInfo = Apps.GoogleDrive.Models.Storage.Responses.FileInfo;

namespace Apps.GoogleDrive.Actions;

[ActionList("Files")]
public class StorageActions : DriveInvocable
{
    private readonly IFileManagementClient _fileManagementClient;

    public StorageActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
    {
        _fileManagementClient = fileManagementClient;
    }

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

    [BlueprintActionDefinition(BlueprintAction.DownloadFile)]
    [Action("Download file", Description = "Download a specific file")]
    public async Task<FileModel> GetFile([ActionParameter] DownloadFileRequest input)
    {
        var request = ExecuteWithErrorHandling(() => Client.Files.Get(input.FileId));
        request.SupportsAllDrives = true;

        var fileMetadata = await ExecuteWithErrorHandlingAsync(request.ExecuteAsync);

        return fileMetadata.MimeType.StartsWith("application/vnd.google-apps")
            ? await DownloadGoogleDocsExport(fileMetadata)
            : await DownloadFileViaPlatform(request, fileMetadata);
    }

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload files")]
    public async Task UploadFile([ActionParameter] UploadFilesRequest input)
    {
        if (input.File.ContentType.Contains("vnd.google-apps"))
        {
            if (!_mimeMap.TryGetValue(input.File.ContentType, out string? contentType))
                throw new PluginMisconfigurationException($"The file {input.File.Name} has type {input.File.ContentType}, which has no defined conversion");

            input.File.ContentType = contentType;
        }

        var body = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.File.Name,
            Parents = [input.ParentFolderId],
            MimeType = String.IsNullOrEmpty(input.SaveAs) ? input.File.ContentType : input.SaveAs,
        };

        await using var fileBytes = await _fileManagementClient.DownloadAsync(input.File);
        var request = ExecuteWithErrorHandling(() => Client.Files.Create(body, fileBytes, input.File.ContentType));
        request.SupportsAllDrives = true;

        var result = await ExecuteWithErrorHandling(() => request.UploadAsync());
        if (result.Status == UploadStatus.Failed)
        {
            throw new PluginApplicationException($"The file upload operation has failed. API error message: {result.Exception.Message}");
        }
    }

    [Action("Delete item", Description = "Delete item (file/folder)")]
    public void DeleteItem([ActionParameter] GetItemRequest input)
    {
        var request = ExecuteWithErrorHandling(() => Client.Files.Delete(input.ItemId));
        request.SupportsAllDrives = true;
        ExecuteWithErrorHandling(() => request.Execute());
    }

    [Action("Search files", Description = "Search files by specific criteria")]
    public async Task<SearchFilesResponse> SearchFilesAsync([ActionParameter] SearchFilesRequest input)
    {
        var query = "trashed = false and mimeType != 'application/vnd.google-apps.folder'";
        
        if (!string.IsNullOrEmpty(input.FolderId))
        {
            query += $" and '{input.FolderId}' in parents";
        }
        
        if (!string.IsNullOrEmpty(input.FileName))
        {
            query += $" and name contains '{input.FileName}'";
        }
        
        if(!string.IsNullOrEmpty(input.MimeType))
        {
            query += $" and mimeType = '{input.MimeType}'";
        }

        var filesListResult = ExecuteWithErrorHandling(() => Client.Files.List());
        filesListResult.IncludeItemsFromAllDrives = true;
        filesListResult.SupportsAllDrives = true;
        filesListResult.Fields = "nextPageToken, files(id, name, createdTime, trashedTime, trashed, modifiedTime, mimeType, size)";
        filesListResult.Q = query;
        
        if(input.Limit.HasValue)
        {
            filesListResult.PageSize = input.Limit.Value;
        }

        var filesList = await ExecuteWithErrorHandlingAsync(async () => await filesListResult.ExecuteAsync());
        var fileDtos = filesList.Files.Select(x => new FileInfo(x)).ToList();

        if (input.FileExactMatch.HasValue && input.FileExactMatch.Value && !String.IsNullOrEmpty(input.FileName))
        {
            if (fileDtos.Any(x => x.FileName == input.FileName))
            {
                fileDtos = fileDtos.Where(x => x.FileName == input.FileName).ToList();
            }
            else 
            {
                return new()
                {
                    Files = new List<FileInfo>(),
                    TotalCount = 0
                };
            }
        }
        return new()
        {
            Files = fileDtos,
            TotalCount = fileDtos.Count
        };
    }
    
    [Action("Get file information", Description = "Get file information by specific criteria")]
    public async Task<FindFileResponse> FindFileAsync([ActionParameter] FindFileRequest input)
    {
        var searchFilesResponse = await ExecuteWithErrorHandlingAsync(async () => await SearchFilesAsync(new SearchFilesRequest
        {
            FolderId = input.FolderId,
            FileName = input.FileName,
            MimeType = input.MimeType,
            FileExactMatch = input.FileExactMatch
        }));
        
        var first = searchFilesResponse.Files.FirstOrDefault();
        return new()
        {
            FileInfo = first ?? new FileInfo(),
            Found = first != null
        };
    }

    [Action("Update file", Description = "Update metadata or content of an existing file without changing the file ID")]
    public async Task UpdateFile([ActionParameter] UpdateFileRequest input)
    {

        var fileMetadata = new Google.Apis.Drive.v3.Data.File();
        if (!string.IsNullOrWhiteSpace(input.NewName))
        {
            string newName = input.NewName;
            if (input.File != null && !newName.Contains("."))
            {
                if (_extensionMap.ContainsKey(input.File.ContentType))
                {
                    newName += _extensionMap[input.File.ContentType];
                }
            }
            fileMetadata.Name = newName;
        }

        MemoryStream memoryStream = null;
        string contentType = null;
        if (input.File != null)
        {
            if (input.File.ContentType.Contains("vnd.google-apps"))
            {
                throw new PluginMisconfigurationException("Updating content for Google Docs files is not supported. Only metadata update is allowed.");
            }

            memoryStream = new MemoryStream();
            using (var downloadStream = await _fileManagementClient.DownloadAsync(input.File))
            {
                await downloadStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;
            contentType = input.File.ContentType;
        }

        if (memoryStream != null)
        {
            var updateMediaRequest = ExecuteWithErrorHandling(()=> Client.Files.Update(fileMetadata, input.FileId, memoryStream, contentType));
            updateMediaRequest.SupportsAllDrives = true;
            var progress = await updateMediaRequest.UploadAsync();
            if (progress.Status == UploadStatus.Failed)
            {
                throw new PluginApplicationException($"The file update operation has failed. API error message: {progress.Exception.Message}");
            }
        }
        else
        {
            var updateRequest = Client.Files.Update(fileMetadata, input.FileId);
            updateRequest.SupportsAllDrives = true;
            await ExecuteWithErrorHandlingAsync(() => updateRequest.ExecuteAsync());
        }
    }

    private async Task<FileModel> DownloadGoogleDocsExport(
        Google.Apis.Drive.v3.Data.File fileMetadata)
    {
        if (!_mimeMap.TryGetValue(fileMetadata.MimeType, out var exportMime))
            throw new PluginMisconfigurationException($"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");

        var exportRequest = ExecuteWithErrorHandling(() => Client.Files.Export(fileMetadata.Id, exportMime));
        var fileName = fileMetadata.Name + _extensionMap[fileMetadata.MimeType];

        // Exports are limited to 10MB, so it's safe to use a MemoryStream here
        using var stream = new MemoryStream();

        ExecuteWithErrorHandling(() => exportRequest.DownloadWithStatus(stream).ThrowOnFailure());

        stream.Position = 0;

        return new FileModel {
            File = await _fileManagementClient.UploadAsync(stream, exportMime, fileName),
        };
    }

    private Task<FileModel> DownloadFileViaPlatform(
        Google.Apis.Drive.v3.FilesResource.GetRequest fileRequest,
        Google.Apis.Drive.v3.Data.File fileMetadata)
    {
        var fileUrl = $"https://www.googleapis.com/drive/v3/files/{fileRequest.FileId}?alt=media";
        var token = InvocationContext.AuthenticationCredentialsProviders.FirstOrDefault(p => p.KeyName == "access_token")?.Value
            ?? throw new PluginApplicationException("Can't create a download request.");

        var downloadRequest = new HttpRequestMessage(HttpMethod.Get, fileUrl);
        downloadRequest.Headers.Authorization = new("Bearer", token);

        return Task.FromResult(new FileModel
        {
            File = new FileReference(downloadRequest, fileMetadata.Name, fileMetadata.MimeType),
        });
    }
}
