using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.Actions;
using Apps.GoogleDrive.Models.Storage.Requests;
using GoogleDriveTests.Base;

namespace Tests.GoogleDrive
{
    [TestClass]
    public class StorageTests : TestBase
    {
        [TestMethod]
        public async Task CreateFolder_IsSuccess()
        {
            var action = new FolderActions(InvocationContext, FileManager);
            var input = new CreateFolderRequest { FolderName = "TestFolder", ParentFolderId = "10Ugy3Y7-kSXNtxFhnESuVpg89WTGl8wB" };
            var result =  action.CreateFolder(input);
            Console.WriteLine(result.FolderID);
            Assert.IsNotNull(result.FolderID);
        }


        [TestMethod]
        public async Task CheckFolderExists_IsSuccess()
        {
            var action = new FolderActions(InvocationContext, FileManager);
            var input = new CheckFolderRequest { FolderName = "TestFolder", ParentFolderId = "10Ugy3Y7-kSXNtxFhnESuVpg89WTGl8wB" };
            var result = await action.CheckFolderExists(input);

            Console.WriteLine(result.Exists);
            Console.WriteLine(result.FolderId);
            Assert.IsTrue(result.Exists);

        }

        [TestMethod]
        public async Task GetFileInfoTest()
        {
            var action = new StorageActions(InvocationContext, FileManager);

            var input = new FindFileRequest { FileName= "23/04/2026_2026Q2S3 UA text ads.json",  FolderId= "145E4tjkkEOXLr38aNBU-FD4wFhRyDvZ5", MimeType= "application/json" };

            var result = await action.FindFileAsync(input);
            //1iZCM6o52QobQK2qPMelx9TphdkKYspnW  
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetFileCommentsTest()
        {
            var action = new StorageActions(InvocationContext, FileManager);
            var input = new GetFileRequest { FileId = "1m9Wfl0h-v7kLi-GAhmXukNtedmEhi7n-" };

            var result = await action.GetFileCommentsAsync(input);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
            Assert.IsNotNull(result);
        }
    }
}
