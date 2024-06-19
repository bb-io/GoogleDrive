using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Storage.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class LabelDataHandler : DriveInvocable, IAsyncDataSourceHandler
    {
        public GetItemRequest GetItemRequest { get; set; }

        public LabelDataHandler(InvocationContext invocationContext, 
            [ActionParameter] GetItemRequest itemRequest) : base(invocationContext)
        {
            GetItemRequest = itemRequest;
        }

        public async Task<Dictionary<string, string>> GetDataAsync(DataSourceContext context, CancellationToken cancellationToken)
        {
            var labels = LabelClient.Labels.List().Execute().Labels;
            if (GetItemRequest.ItemId != null)
            {
                var itemLabels = await Client.Files.ListLabels(GetItemRequest.ItemId).ExecuteAsync();
                return labels.Where(l => itemLabels.Labels.Any(il => il.Id == l.Id)).Where(x => string.IsNullOrWhiteSpace(context.SearchString) ||
                x.Properties.Title.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Name, x => x.Properties.Title);
            }     
            return labels.Where(x => string.IsNullOrWhiteSpace(context.SearchString) || 
            x.Properties.Title.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase) )
                .ToDictionary(x => x.Name, x => x.Properties.Title);
        }
    }
}
