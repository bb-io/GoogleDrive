﻿using System;
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

            var lastInteraction = DateTime.UtcNow.AddHours(-48);
            var memory = new DateMemory { LastInteractionDate = lastInteraction };

            var pollingRequest = new PollingEventRequest<DateMemory>
            {
                Memory = memory,
                PollingTime = DateTime.UtcNow
            };

            var filter = new OnFileCreatedRequest
            {
                FolderId = "1RFZbX3Cg5cxCuP7TFquEpZqlaQLdJWyG"
            };

            var result = await polling.OnFileCreated(pollingRequest, filter);
            foreach (var file in result.Result.Files)
            {
                Console.WriteLine($"File ID: {file.Id}, Name: {file.FileName}");
                Assert.IsNotNull(file);
            }         
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

            Assert.IsNotNull(result);
            foreach (var file in result.Result.Files)
            {
                Console.WriteLine($"File ID: {file.Id}, Name: {file.FileName}");
            }

        }
    }
}
