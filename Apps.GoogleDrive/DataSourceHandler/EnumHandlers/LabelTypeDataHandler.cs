using Blackbird.Applications.Sdk.Common.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.DataSourceHandler.EnumHandlers
{
    public class LabelTypeDataHandler : IStaticDataSourceHandler
    {
        public Dictionary<string, string> GetData()
        {
            return new()
            {
                {"LABEL_TYPE_UNSPECIFIED", "Unknown"},
                {"SHARED", "Shared"},
                {"ADMIN", "Admin"},
            };
        }
    }
}
