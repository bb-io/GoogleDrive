using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.GoogleDrive.Actions
{
    [ActionList]
    public class LabelActions : DriveInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;

        public LabelActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        //[Action("Create label", Description = "Create label")]
        //public async Task<LabelDto> CreateLabel([ActionParameter] CreateLabelRequest createLabelRequest)
        //{
        //    var createRequest = LabelClient.Labels.Create(new GoogleAppsDriveLabelsV2Label()
        //    {
        //        LabelType = createLabelRequest.Type,
        //        Properties = new GoogleAppsDriveLabelsV2LabelProperties()
        //        {
        //            Title = createLabelRequest.Title,
        //            Description = createLabelRequest.Description,
        //        },
        //    });
        //    createRequest.UseAdminAccess = true;
        //    var createRequestResult = await createRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(createRequestResult.Name);
        //    return new(publishResult);
        //}

        //[Action("Get label", Description = "Get label")]
        //public async Task<LabelDto> GetLabel([ActionParameter] GetLabelRequest labelRequest)
        //{
        //    var label = await LabelClient.Labels.Get(labelRequest.LabelId).ExecuteAsync();
        //    return new(label);
        //}

        //[Action("Add text field to label", Description = "Add text field to label")]
        //public async Task<LabelDto> AddTextFieldToLabel(
        //    [ActionParameter] GetLabelRequest labelRequest,
        //    [ActionParameter] AddTextFieldToLabelRequest addTextFieldRequest)
        //{
        //    var updateRequest = LabelClient.Labels.Delta(new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest()
        //    {
        //        UseAdminAccess = true,
        //        Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
        //        {
        //            new()
        //            {
        //                CreateField = new()
        //                {
        //                    Field = new()
        //                    {
        //                        Properties = new()
        //                        {
        //                            DisplayName = addTextFieldRequest.DisplayName
        //                        },
        //                        TextOptions = new()
        //                    }
        //                }
        //            }
        //        }
        //    } ,labelRequest.LabelId);
        //    var updateRequestResult = await updateRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(updateRequestResult.UpdatedLabel.Name);
        //    return new(publishResult);
        //}

        //[Action("Add number field to label", Description = "Add number field to label")]
        //public async Task<LabelDto> AddNumberFieldToLabel(
        //    [ActionParameter] GetLabelRequest labelRequest,
        //    [ActionParameter] AddNumberFieldToLabelRequest addNumberFieldRequest)
        //{
        //    var updateRequest = LabelClient.Labels.Delta(new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest()
        //    {
        //        UseAdminAccess = true,
        //        Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
        //        {
        //            new()
        //            {
        //                CreateField = new()
        //                {
        //                    Field = new()
        //                    {
        //                        Properties = new()
        //                        {
        //                            DisplayName = addNumberFieldRequest.DisplayName
        //                        },
        //                        IntegerOptions = new()
        //                    }
        //                }
        //            }
        //        }
        //    }, labelRequest.LabelId);
        //    var updateRequestResult = await updateRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(updateRequestResult.UpdatedLabel.Name);
        //    return new(publishResult);
        //}

        //[Action("Add date field to label", Description = "Add date field to label")]
        //public async Task<LabelDto> AddDateFieldToLabel(
        //    [ActionParameter] GetLabelRequest labelRequest,
        //    [ActionParameter] AddDateFieldToLabelRequest addDateFieldRequest)
        //{
        //    var updateRequest = LabelClient.Labels.Delta(new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest()
        //    {
        //        UseAdminAccess = true,
        //        Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
        //        {
        //            new()
        //            {
        //                CreateField = new()
        //                {
        //                    Field = new()
        //                    {
        //                        Properties = new()
        //                        {
        //                            DisplayName = addDateFieldRequest.DisplayName
        //                        },
        //                        DateOptions = new()
        //                    }
        //                }
        //            }
        //        }
        //    }, labelRequest.LabelId);
        //    var updateRequestResult = await updateRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(updateRequestResult.UpdatedLabel.Name);
        //    return new(publishResult);
        //}

        //[Action("Add user field to label", Description = "Add user field to label")]
        //public async Task<LabelDto> AddUserFieldToLabel(
        //    [ActionParameter] GetLabelRequest labelRequest,
        //    [ActionParameter] AddUserFieldToLabelRequest addUserFieldRequest)
        //{
        //    var updateRequest = LabelClient.Labels.Delta(new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest()
        //    {
        //        UseAdminAccess = true,
        //        Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
        //        {
        //            new()
        //            {
        //                CreateField = new()
        //                {
        //                    Field = new()
        //                    {
        //                        Properties = new()
        //                        {
        //                            DisplayName = addUserFieldRequest.DisplayName
        //                        },
        //                        UserOptions = new()
        //                    }
        //                }
        //            }
        //        }
        //    }, labelRequest.LabelId);
        //    var updateRequestResult = await updateRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(updateRequestResult.UpdatedLabel.Name);
        //    return new(publishResult);
        //}

        //[Action("Add selection field to label", Description = "Add selection field to label")]
        //public async Task<LabelDto> AddSelectionFieldToLabel(
        //    [ActionParameter] GetLabelRequest labelRequest,
        //    [ActionParameter] AddSelectionFieldToLabelRequest addSelectionFieldRequest)
        //{
        //    var updateRequest = LabelClient.Labels.Delta(new()
        //    {
        //        UseAdminAccess = true,
        //        Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
        //        {
        //            new()
        //            {
        //                CreateField = new()
        //                {
        //                    Field = new()
        //                    {
        //                        Properties = new()
        //                        {
        //                            DisplayName = addSelectionFieldRequest.DisplayName
        //                        },
        //                        SelectionOptions = new(){ Choices = addSelectionFieldRequest.Choices.Select(x => 
        //                        new GoogleAppsDriveLabelsV2FieldSelectionOptionsChoice(){
        //                            Properties = new(){ DisplayName = x }
        //                        }).ToList() }
        //                    }
        //                }
        //            }
        //        }
        //    }, labelRequest.LabelId);
        //    var updateRequestResult = await updateRequest.ExecuteAsync();
        //    var publishResult = await PublishLabel(updateRequestResult.UpdatedLabel.Name);
        //    return new(publishResult);
        //}

        //[Action("Delete label", Description = "Delete label")]
        //public async Task DeleteLabel([ActionParameter] GetLabelRequest labelRequest)
        //{
        //    var disableRequest = LabelClient.Labels.Disable(new GoogleAppsDriveLabelsV2DisableLabelRequest() { UseAdminAccess = true }, labelRequest.LabelId);
        //    await disableRequest.ExecuteAsync();
        //    var deleteRequest = LabelClient.Labels.Delete(labelRequest.LabelId);
        //    deleteRequest.UseAdminAccess = true;
        //    await deleteRequest.ExecuteAsync();
        //}

        //[Action("List labels", Description = "List labels")]
        //public async Task<ListLabelsResponse> ListLabels()
        //{
        //    var labels = LabelClient.Labels.List().Execute();
        //    return new() { Labels = labels.Labels.Select(x => new LabelDto(x)).ToList() };
        //}

        //private async Task<GoogleAppsDriveLabelsV2Label> PublishLabel(string labelId)
        //{
        //    var publishRequest = LabelClient.Labels.Publish(new GoogleAppsDriveLabelsV2PublishLabelRequest()
        //    {
        //        UseAdminAccess = true
        //    }, labelId);
        //    return await publishRequest.ExecuteAsync();
        //}
    }
}
