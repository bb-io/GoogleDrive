using Apps.GoogleDrive.DataSourceHandler.EnumHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using System.Runtime.InteropServices;

namespace Apps.GoogleDrive.Models.Label.Requests
{
    public class CreateLabelRequest
    {
        public string Name { get; set; }

        [Display("Label type")]
        [StaticDataSource(typeof(LabelTypeDataHandler))]
        public string Type { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
    }
}
