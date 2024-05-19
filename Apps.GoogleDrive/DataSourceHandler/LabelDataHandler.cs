using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class LabelDataHandler : DriveInvocable, IDataSourceHandler
    {
        public LabelDataHandler(InvocationContext invocationContext) : base(invocationContext)
        {
        }

        public Dictionary<string, string> GetData(DataSourceContext context)
        {
            var labels = LabelClient.Labels.List().Execute().Labels;
            return labels.Where(x => string.IsNullOrWhiteSpace(context.SearchString) || 
            x.Properties.Title.Contains(context.SearchString, StringComparison.OrdinalIgnoreCase) )
                .ToDictionary(x => x.Name, x => x.Properties.Title);
        }
    }
}
