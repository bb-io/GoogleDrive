using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Apps.GoogleDrive.Utils.StreamWrappers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Blueprints;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Upload;
using System.IO.Pipelines;
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
        request.Fields = "id,name,mimeType,size"; // https://developers.google.com/workspace/drive/api/reference/rest/v3/files#resource:-file
                                                  // We need size for non-Google Docs files to provide to the KnownLengthForwardingStream,
                                                  // but size is not included in the default field set, so we must request it explicitly.
        var fileMetadata = await ExecuteWithErrorHandlingAsync(request.ExecuteAsync);

        try
        {
            return fileMetadata.MimeType.StartsWith("application/vnd.google-apps")
                ? await DownloadGoogleDocsExport(fileMetadata)
                : await DownloadFileViaStreaming(request, fileMetadata);
        }
        catch (Exception ex)
        {
            throw new PluginApplicationException($"TEMP: Error downloading a file: {ex.Message}, stack: {ex.StackTrace?.ToString()}");
        }
    }

    [BlueprintActionDefinition(BlueprintAction.UploadFile)]
    [Action("Upload file", Description = "Upload files")]
    public async Task UploadFile([ActionParameter] UploadFilesRequest input)
    {
        if (input.File.ContentType.Contains("vnd.google-apps"))
        {
            //if (!string.IsNullOrWhiteSpace(input.SaveAs))
            //{
            //    input.File.ContentType = input.SaveAs;
            //}
            //else
            //{
                if (!_mimeMap.ContainsKey(input.File.ContentType))
                    throw new PluginMisconfigurationException(
                        $"The file {input.File.Name} has type {input.File.ContentType}, which has no defined conversion");

                input.File.ContentType = _mimeMap[input.File.ContentType];
           // }
        }
        var body = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.File.Name,
            Parents = new List<string> { input.ParentFolderId },
            MimeType = String.IsNullOrEmpty(input.SaveAs)?input.File.ContentType:input.SaveAs
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
        var query = "mimeType != 'application/vnd.google-apps.folder'";
        
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

    private async Task<FileModel> DownloadGoogleDocsExport(Google.Apis.Drive.v3.Data.File fileMetadata)
    {
        if (!_mimeMap.TryGetValue(fileMetadata.MimeType, out var exportMime))
            throw new PluginMisconfigurationException($"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");

        var exportRequest = ExecuteWithErrorHandling(() => Client.Files.Export(fileMetadata.Id, exportMime));
        var fileName = fileMetadata.Name + _extensionMap[fileMetadata.MimeType];

        using var limitedStream = new LimitedMemoryStream();
        ExecuteWithErrorHandling(() =>
            exportRequest.DownloadWithStatus(limitedStream).ThrowOnFailure());

        limitedStream.Position = 0;

        return new FileModel {
            File = await _fileManagementClient.UploadAsync(limitedStream, exportMime, fileName),
        };
    }

    private async Task<FileModel> DownloadFileViaStreaming(Google.Apis.Drive.v3.FilesResource.GetRequest fileRequest, Google.Apis.Drive.v3.Data.File fileMetadata)
    {
        if (fileMetadata.Size is null)
            throw new PluginApplicationException("File size is not available in metadata (can't download a folder or a shortcut).");

        var pipe = new Pipe();
        var size = fileMetadata.Size.Value;

        // Producer: download from Google Drive into the pipe
        var downloadTask = Task.Run(async () =>
        {
            Exception? error = null;
            try
            {
                // Google API will write all bytes then return
                await fileRequest.DownloadAsync(pipe.Writer.AsStream());
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                // Complete the writer so the reader sees EOF (or error)
                await pipe.Writer.CompleteAsync(error);
            }
        });

        FileReference uploadedFileReference;
        try
        {
            // Consumer: upload while streaming from the pipe
            await using var readerStream = pipe.Reader.AsStream();
            using var knownLengthStream = new KnownLengthForwardingStream(readerStream, size);

            uploadedFileReference = await _fileManagementClient
                .UploadAsync(knownLengthStream, fileMetadata.MimeType, fileMetadata.Name);
        }
        finally
        {
            pipe.Reader.Complete();
        }

        // Ensure download finished and propagate any download error
        await downloadTask;

        return new FileModel
        {
            File = uploadedFileReference
        };
    }
}
