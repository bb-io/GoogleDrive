using Apps.GoogleDrive.Dtos;
using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Apps.GoogleDrive.Models.Label.Responses;
using Blackbird.Applications.Sdk.Common;
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

        [Action("Create label", Description = "Create label")]
        public async Task<LabelDto> CreateLabel([ActionParameter] CreateLabelRequest createLabelRequest)
        {
            var createRequest = LabelClient.Labels.Create(new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2Label()
            {
                LabelType = createLabelRequest.Type,
                Properties = new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2LabelProperties()
                {
                    Title = createLabelRequest.Title,
                    Description = createLabelRequest.Description,
                },
            });
            createRequest.UseAdminAccess = true;
            var createRequestResult = await createRequest.ExecuteAsync();

            var publishRequest = LabelClient.Labels.Publish(new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2PublishLabelRequest()
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

        [Action("Delete label", Description = "Delete label")]
        public async Task DeleteLabel([ActionParameter] GetLabelRequest labelRequest)
        {
            await LabelClient.Labels.Delete(labelRequest.LabelId).ExecuteAsync();
        }

        [Action("List labels", Description = "List labels")]
        public async Task<ListLabelsResponse> ListLabels()
        {
            var labels = LabelClient.Labels.List().Execute();
            return new() { Labels = labels.Labels.Select(x => new LabelDto(x)).ToList() };
        }
    }
}
