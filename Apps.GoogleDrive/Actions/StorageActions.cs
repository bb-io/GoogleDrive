using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models;
using Apps.GoogleDrive.Models.Label.Responses;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Upload;
using FileInfo = Apps.GoogleDrive.Models.Storage.Responses.FileInfo;

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

    [Action("Download file", Description = "Download a specific file")]
    public async Task<FileModel> GetFile([ActionParameter] GetFilesRequest input)
    {
        var request = Client.Files.Get(input.FileId);
        request.SupportsAllDrives = true;

        var fileMetadata = await ExecuteSafeAsync(() => request.ExecuteAsync());

        byte[] data;
        var fileName = fileMetadata.Name;
        var mimeType = fileMetadata.MimeType;


        using (var stream = new MemoryStream())
        {
            if (fileMetadata.MimeType.Contains("vnd.google-apps"))
            {
                if (!_mimeMap.ContainsKey(fileMetadata.MimeType))
                    throw new Exception(
                        $"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");
                var exportRequest = Client.Files.Export(input.FileId, _mimeMap[fileMetadata.MimeType]);
                exportRequest.DownloadWithStatus(stream).ThrowOnFailure();
                fileName += _extensionMap[fileMetadata.MimeType];
                mimeType = _mimeMap[fileMetadata.MimeType];
            }
            else
            {
                await ExecuteSafeAsync<bool>(() =>
                {
                    request.DownloadWithStatus(stream).ThrowOnFailure();
                    return Task.FromResult(true);
                });
            }  

            data = stream.ToArray();
        }

        using var stream2 = new MemoryStream(data);

        return new()
        {
            File = await _fileManagementClient.UploadAsync(stream2, mimeType, fileName)
        };
    }

    [Action("Upload file", Description = "Upload files")]
    public async Task UploadFile([ActionParameter] UploadFilesRequest input)
    {
        var body = new Google.Apis.Drive.v3.Data.File
        {
            Name = input.File.Name,
            Parents = new List<string> { input.ParentFolderId }
        };

        if (input.File.ContentType.Contains("vnd.google-apps"))
        {
            if (!string.IsNullOrWhiteSpace(input.SaveAs))
            {
                input.File.ContentType = input.SaveAs;
            }
            else
            {
                if (!_mimeMap.ContainsKey(input.File.ContentType))
                    throw new Exception(
                        $"The file {input.File.Name} has type {input.File.ContentType}, which has no defined conversion");

                input.File.ContentType = _mimeMap[input.File.ContentType];
            }
        }

        await using var fileBytes = await _fileManagementClient.DownloadAsync(input.File);
        var request = Client.Files.Create(body, fileBytes, input.File.ContentType);
        request.SupportsAllDrives = true;

        var result = await request.UploadAsync();
        if (result.Status == UploadStatus.Failed)
        {
            throw new PluginApplicationException($"The file upload operation has failed. API error message: {result.Exception.Message}");
        }
    }

    [Action("Delete item", Description = "Delete item (file/folder)")]
    public void DeleteItem([ActionParameter] GetItemRequest input)
    {
        var request = Client.Files.Delete(input.ItemId);
        request.SupportsAllDrives = true;
        request.Execute();
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

        var filesListResult = Client.Files.List();
        filesListResult.IncludeItemsFromAllDrives = true;
        filesListResult.SupportsAllDrives = true;
        filesListResult.Fields = "nextPageToken, files(id, name, createdTime, trashedTime, trashed, modifiedTime, mimeType, size)";
        filesListResult.Q = query;
        
        if(input.Limit.HasValue)
        {
            filesListResult.PageSize = input.Limit.Value;
        }

        var filesList = await filesListResult.ExecuteAsync();
        var fileDtos = filesList.Files.Select(x => new FileInfo(x)).ToList();
        return new()
        {
            Files = fileDtos,
            TotalCount = fileDtos.Count
        };
    }
    
    [Action("Get file information", Description = "Get file information by specific criteria")]
    public async Task<FindFileResponse> FindFileAsync([ActionParameter] FindFileRequest input)
    {
        var searchFilesResponse = await SearchFilesAsync(new SearchFilesRequest
        {
            FolderId = input.FolderId,
            FileName = input.FileName,
            MimeType = input.MimeType
        });
        
        var first = searchFilesResponse.Files.FirstOrDefault();
        return new()
        {
            FileInfo = first ?? new FileInfo(),
            Found = first != null
        };
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
        var response = ExecuteSafeAsync(() => request.ExecuteAsync())
                     .GetAwaiter()
                     .GetResult();
        return new CreateFolderResponse
        { 
            FolderID = response.Id,
            FolderName = response.Name
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

        var response = await listRequest.ExecuteAsync();

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

    #endregion

    #region Labels actions

    [Action("Get file labels", Description = "Returns all the label field keys attached to a file")]
    public async Task<FieldKeysResponse> GetFileLabels([ActionParameter] GetFilesRequest input)
    {
        var res = await Client.Files.ListLabels(input.FileId).ExecuteAsync();
        var fieldKeys = res.Labels.SelectMany(x => x.Fields.Select(y => y.Key));
        return new FieldKeysResponse
        {
            Keys = fieldKeys ?? new List<string>(),
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
                throw new Exception("Updating content for Google Docs files is not supported. Only metadata update is allowed.");
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
            var updateMediaRequest = Client.Files.Update(fileMetadata, input.FileId, memoryStream, contentType);
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
            await ExecuteSafeAsync(() => updateRequest.ExecuteAsync());
        }
    }



    //[Action("Add label to item", Description = "Add label to item (file/folder)")]
    //public async Task AddLabelToItem(
    //    [ActionParameter] GetItemRequest itemRequest, 
    //    [ActionParameter] GetLabelRequest labelsRequest)
    //{
    //    var request = Client.Files.ModifyLabels(
    //        new ModifyLabelsRequest()
    //        {
    //            LabelModifications = new List<LabelModification>() { 
    //                new LabelModification() { 
    //                    LabelId = labelsRequest.LabelId, 
    //                } 
    //            },
    //        }, itemRequest.ItemId);
    //    await request.ExecuteAsync();
    //}

    //[Action("Remove label from item", Description = "Add labels to item (file/folder)")]
    //public async Task RemoveLabelFromItem(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest)
    //{
    //    var request = Client.Files.ModifyLabels(
    //        new ModifyLabelsRequest()
    //        {
    //            LabelModifications = new List<LabelModification>() {
    //                new LabelModification() {
    //                    LabelId = labelsRequest.LabelId, 
    //                    RemoveLabel = true
    //                }
    //            },
    //        }, itemRequest.ItemId);
    //    await request.ExecuteAsync();
    //}

    //[Action("Set label text field", Description = "Set label text field")]
    //public async Task SetLabelText(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest,
    //    [ActionParameter] SetLabelTextRequest labelTextRequest)
    //{
    //    var labelFieldModification = new LabelFieldModification()
    //    {
    //        FieldId = labelTextRequest.FieldId,
    //        SetTextValues = new List<string>() { labelTextRequest.TextFieldValue }
    //    };
    //    await SetLabelField(itemRequest.ItemId, labelsRequest.LabelId, labelFieldModification);
    //}

    //[Action("Set label number field", Description = "Set label number field")]
    //public async Task SetLabelNumber(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest,
    //    [ActionParameter] SetLabelNumberRequest labelNumberRequest)
    //{
    //    var labelFieldModification = new LabelFieldModification()
    //    {
    //        FieldId = labelNumberRequest.FieldId,
    //        SetIntegerValues = new List<long?>() { labelNumberRequest.NumberFieldValue }
    //    };
    //    await SetLabelField(itemRequest.ItemId, labelsRequest.LabelId, labelFieldModification);
    //}

    //[Action("Set label date field", Description = "Set label date field")]
    //public async Task SetLabelDate(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest,
    //    [ActionParameter] SetLabelDateRequest labelDateRequest)
    //{
    //    var labelFieldModification = new LabelFieldModification()
    //    {
    //        FieldId = labelDateRequest.FieldId,
    //        SetDateValues = new List<string>() { labelDateRequest.DateFieldValue.ToString("YYYY-MM-dd") }
    //    };
    //    await SetLabelField(itemRequest.ItemId, labelsRequest.LabelId, labelFieldModification);
    //}

    //[Action("Set label user field", Description = "Set label user field")]
    //public async Task SetLabelUser(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest,
    //    [ActionParameter] SetLabelUserRequest labelUserRequest)
    //{
    //    var labelFieldModification = new LabelFieldModification()
    //    {
    //        FieldId = labelUserRequest.FieldId,
    //        SetUserValues = new List<string>() { labelUserRequest.UserFieldValue }
    //    };
    //    await SetLabelField(itemRequest.ItemId, labelsRequest.LabelId, labelFieldModification);
    //}

    //[Action("Set label selection field", Description = "Set label selection field")]
    //public async Task SetLabelSelection(
    //    [ActionParameter] GetItemRequest itemRequest,
    //    [ActionParameter] GetLabelRequest labelsRequest,
    //    [ActionParameter] SetLabelFieldBaseRequest labelFieldRequest,
    //    [ActionParameter] SetLabelSelectionRequest labelSelectionRequest)
    //{
    //    var labelFieldModification = new LabelFieldModification()
    //    {
    //        FieldId = labelFieldRequest.FieldId,
    //        SetSelectionValues = new List<string>() { labelSelectionRequest.SelectionFieldValue }
    //    };
    //    await SetLabelField(itemRequest.ItemId, labelsRequest.LabelId, labelFieldModification);
    //}

    //private async Task SetLabelField(string itemId, string labelId, LabelFieldModification fieldModification)
    //{
    //    var request = Client.Files.ModifyLabels(
    //        new ModifyLabelsRequest()
    //        {
    //            LabelModifications = new List<LabelModification>() {
    //                new LabelModification() {
    //                    LabelId = labelId,
    //                    FieldModifications = new List<LabelFieldModification>()
    //                    {
    //                        fieldModification
    //                    }
    //                }
    //            },
    //        }, itemId);
    //    await request.ExecuteAsync();
    //}

    #endregion
}