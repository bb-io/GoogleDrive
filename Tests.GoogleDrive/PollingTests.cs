using Apps.GoogleDrive.Polling;
using Apps.GoogleDrive.Polling.Models;
using Apps.GoogleDrive.Polling.Models.Memory;
using Blackbird.Applications.Sdk.Common.Polling;
using GoogleDriveTests.Base;

namespace Tests.GoogleDrive;

[TestClass]
public class PollingTests : TestBase
{
    [TestMethod]
    public async Task OnFileCreated_ReturnsCreatedFiles()
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
            FolderId = "1ZgCDIk5R2IDhe2i5uEKeWPROHSnKAj8z",
            IncludeSubfolders = true,
            MaxSubfolderLevel = 2
        };

        var result = await polling.OnFileCreated(pollingRequest, filter);
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task OnFileUpdated_IsSuccess()
    {
        var polling = new PollingList(InvocationContext);

        var lastInteraction = DateTime.UtcNow.AddHours(-2);

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
            FolderId = "1ZgCDIk5R2IDhe2i5uEKeWPROHSnKAj8z",
            IncludeSubfolders = true,
        };
        var result = await polling.OnFileUpdated(filter, pollingRequest);

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine(json);
        Assert.IsNotNull(result);
    }
}
