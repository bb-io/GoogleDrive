using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.GoogleDrive.DataSourceHandler.EnumHandlers
{
    public class LabelTypeDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new()
            {
               // {"LABEL_TYPE_UNSPECIFIED", "Unknown"},
                {"SHARED", "Shared"},
                {"ADMIN", "Admin"},
            };
        }
    }
}
