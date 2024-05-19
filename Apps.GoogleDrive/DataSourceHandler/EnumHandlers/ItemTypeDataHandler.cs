using Blackbird.Applications.Sdk.Common.Dictionaries;

namespace Apps.GoogleDrive.DataSourceHandler.EnumHandlers
{
    public class ItemTypeDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new()
            {
                { "file", "File" },
                { "folder", "Folder" },
                { "both", "Both" }
            };
        }
    }
}
