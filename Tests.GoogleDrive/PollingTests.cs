using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.Polling;
using Apps.GoogleDrive.Polling.Models;
using Apps.GoogleDrive.Polling.Models.Memory;
using Blackbird.Applications.Sdk.Common.Polling;
using GoogleDriveTests.Base;

namespace Tests.GoogleDrive
{
    [TestClass]
    public class PollingTests:TestBase
    {
        [TestMethod]
        public async Task OnFileCreated_Succes()
        {
            var polling = new PollingList(InvocationContext);

            var lastInteraction = DateTime.UtcNow.AddHours(-2);
            var memory = new DateMemory { LastInteractionDate = lastInteraction };

            var pollingRequest = new PollingEventRequest<DateMemory>
            {
                Memory = memory,
                PollingTime = DateTime.UtcNow
            };

            var filter = new OnFileCreatedRequest
            {
                //FolderId = "1-2eaBDlwP-8MbgLqlKnSWCl8RJe3vTEU"
                FolderId = "1he8_Zv_a6YW1PNarlPjBBZjT3JE4n1rj",
                //MimeType= "application/vnd.google-apps.spreadsheet",
                MimeType= "image/png",
                FileNameContains= "2026"
            };

            var result = await polling.OnFileCreated(pollingRequest, filter);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            Console.WriteLine(json);
            Assert.IsNotNull(result);
        }



        [TestMethod]
        public async Task OnFileUpdated_IsSuccess()
        {
            var polling = new PollingList(InvocationContext);

            var lastInteraction = DateTime.UtcNow.AddMinutes(-30);

            var memory = new DateMemory
            {
                LastInteractionDate = lastInteraction
            };

            var pollingRequest = new PollingEventRequest<DateMemory>
            {
                Memory = memory,
                PollingTime = DateTime.UtcNow
            };
            var filter = new OnFileUpdateRequest
            {
                //FolderId = "1RFZbX3Cg5cxCuP7TFquEpZqlaQLdJWyG",
                FileId = "11aOSxiOObE0D2PKQF1NbOLZXRPXKUuXk"
            };
            var result = await polling.OnFileUpdated(filter, pollingRequest);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            Console.WriteLine(json);
            Assert.IsNotNull(result);
        }
    }
}
