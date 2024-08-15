using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Apps.GoogleDrive.Models.Storage.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;
using Google.Apis.DriveLabels.v2.Data;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class LabelTypedFieldDataHandler : DriveInvocable, IAsyncDataSourceHandler
    {
        public GetItemRequest GetItemRequest { get; set; }
        public GetLabelRequest GetLabelRequest { get; set; }

        public LabelTypedFieldDataHandler(InvocationContext invocationContext, 
            [ActionParameter] GetItemRequest itemRequest,
            [ActionParameter] GetLabelRequest labelsRequest) : base(invocationContext)
        {
            GetItemRequest = itemRequest;
            GetLabelRequest = labelsRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            //if (GetItemRequest.ItemId == null)
            //    throw new ArgumentException("Select storage item first, please");

            //if (GetLabelRequest.LabelId == null)
            //    throw new ArgumentException("Select label, please");

            if (GetItemRequest.ItemId != null)
            {
                var itemLabels = await Client.Files.ListLabels(GetItemRequest.ItemId).ExecuteAsync();
                if (GetLabelRequest.LabelId != null)
                {
                    var label = itemLabels.Labels.FirstOrDefault(x => GetLabelRequest.LabelId.Contains(x.Id));

                    var labelFullRequest = LabelClient.Labels.Get(GetLabelRequest.LabelId);
                    labelFullRequest.View = Google.Apis.DriveLabels.v2.LabelsResource.GetRequest.ViewEnum.LABELVIEWFULL;
                    var labelFull = await labelFullRequest.ExecuteAsync();

                    return labelFull.Fields.Where(x => string.IsNullOrWhiteSpace(context.SearchString) || 
                    x.Properties.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(k => k.Id, v => v.Properties.DisplayName);
                }
                var labelsRequest = LabelClient.Labels.List();
                labelsRequest.View = Google.Apis.DriveLabels.v2.LabelsResource.ListRequest.ViewEnum.LABELVIEWFULL;
                var labels = await labelsRequest.ExecuteAsync();

                return labels.Labels.Where(lab => itemLabels.Labels.Any(itemLab => itemLab.Id == lab.Id)).SelectMany(l => l.Fields.Where(x => string.IsNullOrWhiteSpace(context.SearchString) ||
                    x.Properties.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .Select(y => new KeyValuePair<GoogleAppsDriveLabelsV2Label, GoogleAppsDriveLabelsV2Field>(l, y))).ToDictionary(k => k.Value.Id, v => $"{v.Value.Properties.DisplayName} ({v.Key.Properties.Title})");
            }
            var labelsAllRequest = LabelClient.Labels.List();
            labelsAllRequest.View = Google.Apis.DriveLabels.v2.LabelsResource.ListRequest.ViewEnum.LABELVIEWFULL;
            var labelsAll = await labelsAllRequest.ExecuteAsync();

            return labelsAll.Labels.SelectMany(l => l.Fields.Where(x => string.IsNullOrWhiteSpace(context.SearchString) ||
                x.Properties.DisplayName.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
            .Select(y => new KeyValuePair<GoogleAppsDriveLabelsV2Label, GoogleAppsDriveLabelsV2Field>(l, y))).ToDictionary(k => k.Value.Id, v => $"{v.Value.Properties.DisplayName} ({v.Key.Properties.Title})");
        }
    }
}
