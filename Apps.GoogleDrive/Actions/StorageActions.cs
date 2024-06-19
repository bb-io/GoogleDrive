using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Apps.GoogleDrive.Models.Storage.Requests;
using Apps.GoogleDrive.Models.Storage.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.Download;
using Google.Apis.Drive.v3.Data;
using Google.Apis.DriveActivity.v2.Data;

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

    [Action("Download files", Description = "Download files")]
    public async Task<GetFilesResponse> GetFile([ActionParameter] GetFilesRequest input)
    {
        var fileReferences = new List<FileReference>();

        foreach (var fileId in input.FileIds)
        {
            var request = Client.Files.Get(fileId);
            request.SupportsAllDrives = true;
            var fileMetadata = await request.ExecuteAsync();

            byte[] data;
            var fileName = fileMetadata.Name;
            using (var stream = new MemoryStream())
            {
                if (fileMetadata.MimeType.Contains("vnd.google-apps"))
                {
                    if (!_mimeMap.ContainsKey(fileMetadata.MimeType))
                        throw new Exception($"The file {fileMetadata.Name} has type {fileMetadata.MimeType}, which has no defined conversion");
                    var exportRequest = Client.Files.Export(fileId, _mimeMap[fileMetadata.MimeType]);
                    exportRequest.DownloadWithStatus(stream).ThrowOnFailure();
                    fileName += _extensionMap[fileMetadata.MimeType];
                }
                else
                    request.DownloadWithStatus(stream).ThrowOnFailure();

                data = stream.ToArray();
            }
            using var stream2 = new MemoryStream(data);
            var file = await _fileManagementClient.UploadAsync(stream2, fileMetadata.MimeType, fileName);
            fileReferences.Add(file);
        }

        return new() { Files = fileReferences };
    }

    [Action("Upload files", Description = "Upload files")]
    public async Task UploadFile([ActionParameter] UploadFilesRequest input)
    {
        foreach (var file in input.Files)
        {
            var body = new Google.Apis.Drive.v3.Data.File
            {
                Name = file.Name,
                Parents = new List<string> { input.ParentFolderId }
            };

            await using var fileBytes = await _fileManagementClient.DownloadAsync(file);
            var request = Client.Files.Create(body, fileBytes, null);
            await request.UploadAsync();
        }
    }

    [Action("Delete item", Description = "Delete item (file/folder)")]
    public void DeleteItem([ActionParameter] GetItemRequest input)
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