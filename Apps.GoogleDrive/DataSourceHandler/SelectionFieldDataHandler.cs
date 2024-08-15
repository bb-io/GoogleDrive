using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class SelectionFieldDataHandler : DriveInvocable, IAsyncDataSourceHandler
    {
        public SetLabelFieldBaseRequest GetFieldRequest { get; set; }
        public GetLabelRequest GetLabelRequest { get; set; }

        public SelectionFieldDataHandler(InvocationContext invocationContext,
            [ActionParameter] GetLabelRequest labelsRequest,
            [ActionParameter] SetLabelFieldBaseRequest fieldRequest) : base(invocationContext)
        {
            GetFieldRequest = fieldRequest;
            GetLabelRequest = labelsRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            if (GetLabelRequest.LabelId == null)
                throw new ArgumentNullException("Select label first");

            if (GetFieldRequest.FieldId == null)
                throw new ArgumentNullException("Select field first");

            var labelFullRequest = LabelClient.Labels.Get(GetLabelRequest.LabelId);
            labelFullRequest.View = Google.Apis.DriveLabels.v2.LabelsResource.GetRequest.ViewEnum.LABELVIEWFULL;
            var labelFull = await labelFullRequest.ExecuteAsync();

            return labelFull.Fields.FirstOrDefault(x => x.Id == GetFieldRequest.FieldId).SelectionOptions.Choices.ToDictionary(k => k.Id, v => v.Properties.DisplayName);
        }
    }
}
