using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.GoogleDrive.Actions;
using Apps.GoogleDrive.Models.Storage.Requests;
using Blackbird.Applications.Sdk.Common.Files;
using Blackbird.Applications.Sdk.Common.Invocation;
using GoogleDriveTests.Base;

namespace Tests.GoogleDrive
{
    [TestClass]
    public class FileTests:TestBase
    {
        [TestMethod]
        public async Task FileDownloadTest()
        {
            var action = new StorageActions(InvocationContext,FileManager);

            var input = new DownloadFileRequest { FileId = "1iZCM6o52QobQK2qPMelx9TphdkKYspnW" };

            var result = await action.GetFile(input);
            //1iZCM6o52QobQK2qPMelx9TphdkKYspnW  

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task FileUploadTest()
        {
            var action = new StorageActions(InvocationContext, FileManager);

            var input = new UploadFilesRequest { File= new FileReference { Name= "test.txt", ContentType= "text/plain" } , ParentFolderId= "10Ugy3Y7-kSXNtxFhnESuVpg89WTGl8wB", SaveAs= "application/pdf" };

            await action.UploadFile(input);

            Assert.IsTrue(true);
        }


        [TestMethod]
        public async Task FileUpdateTest()
        {
            var action = new StorageActions(InvocationContext, FileManager);

            var input = new UpdateFileRequest {  FileId= "10HIlZ_1rAtpGhw3J_TixKUTFnu6waHau", File = new FileReference { Name = "test.txt", ContentType = "text/plain" } };

            await action.UpdateFile(input);

            Assert.IsTrue(true);
        }
    }
}
