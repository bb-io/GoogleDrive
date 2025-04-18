﻿using System;
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
            var action = new StorageActions(InvocationContext, FileManager);
            var input = new CreateFolderRequest { FolderName = "TestFolder", ParentFolderId = "10Ugy3Y7-kSXNtxFhnESuVpg89WTGl8wB" };
            var result =  action.CreateFolder(input);
            Console.WriteLine(result.FolderID);
            Assert.IsNotNull(result.FolderID);
        }


        [TestMethod]
        public async Task CheckFolderExists_IsSuccess()
        {
            var action = new StorageActions(InvocationContext, FileManager);
            var input = new CheckFolderRequest { FolderName = "TestFolder", ParentFolderId = "10Ugy3Y7-kSXNtxFhnESuVpg89WTGl8wB" };
            var result = await action.CheckFolderExists(input);

            Console.WriteLine(result.Exists);
            Console.WriteLine(result.FolderId);
            Assert.IsTrue(result.Exists);

        }
    }
}
