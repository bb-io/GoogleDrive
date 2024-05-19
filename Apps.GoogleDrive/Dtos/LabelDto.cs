using Blackbird.Applications.Sdk.Common;
using Google.Apis.DriveLabels.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.Dtos
{
    public class LabelDto
    {
        public LabelDto(GoogleAppsDriveLabelsV2Label label) 
        {
            Id = label.Id;
            Name = label.Name;
            Title = label.Properties.Title;
            Description = label.Properties.Description;
            LabelType = label.LabelType;
            State = label.Lifecycle.State;
        }
        public string Id { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [Display("Label type")]
        public string LabelType { get; set; }
        public string State { get; set; }
    }
}
