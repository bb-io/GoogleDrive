using Apps.GoogleDrive.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.Models.Label.Responses
{
    public class ListLabelsResponse
    {
        public List<LabelDto> Labels { get; set; }
    }
}
