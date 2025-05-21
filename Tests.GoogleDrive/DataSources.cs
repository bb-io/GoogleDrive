using Apps.GoogleDrive.DataSourceHandler;
using Blackbird.Applications.Sdk.Common.Dynamic;
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

            var response =  handler.GetData(new DataSourceContext { SearchString= "original" });

            foreach (var file in response)
            {
                Console.WriteLine($"{file.Key} - {file.Value}");
            }
            Assert.IsNotNull(response);
        }

    }
}
