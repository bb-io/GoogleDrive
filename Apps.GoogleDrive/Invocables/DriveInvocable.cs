﻿using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.Invocables
{
    public class DriveInvocable : BaseInvocable
    {
        protected GoogleDriveClient Client { get; }

        protected GoogleDriveLabelClient LabelClient { get; }

        public DriveInvocable(InvocationContext invocationContext) : base(invocationContext)
        {
            Client = new GoogleDriveClient(InvocationContext.AuthenticationCredentialsProviders);
            //LabelClient = new GoogleDriveLabelClient(InvocationContext.AuthenticationCredentialsProviders);
        }
    }
}
