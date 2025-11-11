using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using GoogleDriveTests.Base;

namespace Tests.GoogleDrive
{
    [TestClass]
    public class DataSources : TestBase
    {
        [TestMethod]
        public async Task FileDataHandlerReturnsValues()
        {
            var handler = new FileDataHandler(InvocationContext);

            var response =  handler.GetData(new DataSourceContext { SearchString= "Стальна" });

            foreach (var file in response)
            {
                Console.WriteLine($"{file.Key} - {file.Value}");
            }
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task FolderDataHandlerReturnsValues()
        {
            var handler = new FolderDataHandler(InvocationContext);

            var response = handler.GetData(new DataSourceContext { SearchString = "" });

            foreach (var file in response)
            {
                Console.WriteLine($"{file.Key} - {file.Value}");
            }
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task FilePickerDataHandler_IsSuccess()
        {
            var handler = new FilePickerDataSourceHandler(InvocationContext);
            var result = await handler.GetFolderContentAsync(new FolderContentDataSourceContext
            {
                FolderId = string.Empty
            }, CancellationToken.None);
            var itemList = result.ToList();
            foreach (var item in itemList)
            {
                Console.WriteLine($"Item: {item.DisplayName}, Id: {item.Id}, Type: {(item is Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder ? "Folder" : "File")}");
            }
            Assert.IsNotNull(result);
            Assert.IsTrue(itemList.Count > 0, "The folder should contain items.");
        }

        [TestMethod]
        public async Task FolderPickerDataHandler_IsSuccess()
        {
            var handler = new FolderPickerDataSourceHandler(InvocationContext);
            var result = await handler.GetFolderContentAsync(new FolderContentDataSourceContext
            {
                FolderId = string.Empty
            }, CancellationToken.None);
            var itemList = result.ToList();
            foreach (var item in itemList)
            {
                Console.WriteLine($"Item: {item.DisplayName}, Id: {item.Id}, Type: {(item is Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder ? "Folder" : "File")}");
            }
            Assert.IsNotNull(result);
            Assert.IsTrue(itemList.Count > 0, "The folder should contain items.");
        }
    }
}
