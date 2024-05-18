﻿using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Label.Requests;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;

namespace Apps.GoogleDrive.Actions
{
    public class LabelActions : DriveInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;

        public LabelActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Create label", Description = "Create label")]
        public async Task CreateLabel([ActionParameter] CreateLabelRequest createLabelRequest)
        {
            LabelClient.Labels.Create(new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2Label()
            {
                Name = createLabelRequest.Name,
                LabelType = createLabelRequest.Type,
                Properties = new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2LabelProperties()
                {
                    Title = createLabelRequest.Title,
                    Description = createLabelRequest.Description,
                },
            });
            LabelClient.Labels.Publish(new Google.Apis.DriveLabels.v2.Data.GoogleAppsDriveLabelsV2PublishLabelRequest()
            {

            }, createLabelRequest.Name);
        } 
    }
}
