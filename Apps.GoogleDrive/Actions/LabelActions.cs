using Apps.GoogleDrive.Dtos;
using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Apps.GoogleDrive.Models.Label.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Google.Apis.DriveLabels.v2.Data;

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

        [Action("Create label", Description = "Create label")]
        public async Task<LabelDto> CreateLabel([ActionParameter] CreateLabelRequest createLabelRequest)
        {
            var createRequest = LabelClient.Labels.Create(new GoogleAppsDriveLabelsV2Label()
            {
                LabelType = createLabelRequest.Type,
                Properties = new GoogleAppsDriveLabelsV2LabelProperties()
                {
                    Title = createLabelRequest.Title,
                    Description = createLabelRequest.Description,
                },
            });
            createRequest.UseAdminAccess = true;
            var createRequestResult = await createRequest.ExecuteAsync();

            var publishRequest = LabelClient.Labels.Publish(new GoogleAppsDriveLabelsV2PublishLabelRequest()
            {
                UseAdminAccess = true
            }, createRequestResult.Name);
            var publishResult = await publishRequest.ExecuteAsync();
            return new(publishResult);
        }

        [Action("Get label", Description = "Get label")]
        public async Task<LabelDto> GetLabel([ActionParameter] GetLabelRequest labelRequest)
        {
            var label = await LabelClient.Labels.Get(labelRequest.LabelId).ExecuteAsync();
            return new(label);
        }

        [Action("Add text field to label", Description = "Add text field to label")]
        public async Task AddTextFieldToLabel(
            [ActionParameter] GetLabelRequest labelRequest,
            [ActionParameter] AddTextFieldToLabelRequest addTextFieldRequest)
        {
            var updateRequest = LabelClient.Labels.Delta(new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequest()
            {
                UseAdminAccess = true,
                Requests = new List<GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest>()
                {
                    new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestRequest()
                    {
                        CreateField = new GoogleAppsDriveLabelsV2DeltaUpdateLabelRequestCreateFieldRequest()
                        {
                            Field = new GoogleAppsDriveLabelsV2Field()
                            {
                                Properties = new GoogleAppsDriveLabelsV2FieldProperties()
                                {
                                    DisplayName = addTextFieldRequest.DisplayName
                                },
                                TextOptions = new()
                            }
                        }
                    }
                }
            } ,labelRequest.LabelId);
            await updateRequest.ExecuteAsync();
        }

        [Action("Delete label", Description = "Delete label")]
        public async Task DeleteLabel([ActionParameter] GetLabelRequest labelRequest)
        {
            var disableRequest = LabelClient.Labels.Disable(new GoogleAppsDriveLabelsV2DisableLabelRequest() { UseAdminAccess = true }, labelRequest.LabelId);
            await disableRequest.ExecuteAsync();
            var deleteRequest = LabelClient.Labels.Delete(labelRequest.LabelId);
            deleteRequest.UseAdminAccess = true;
            await deleteRequest.ExecuteAsync();
        }

        [Action("List labels", Description = "List labels")]
        public async Task<ListLabelsResponse> ListLabels()
        {
            var labels = LabelClient.Labels.List().Execute();
            return new() { Labels = labels.Labels.Select(x => new LabelDto(x)).ToList() };
        }
    }
}
