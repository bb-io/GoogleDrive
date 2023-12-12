using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.GoogleDrive.Invocables
{
    public class DriveInvocable : BaseInvocable
    {
        protected GoogleDriveClient Client { get; }

        public DriveInvocable(InvocationContext invocationContext) : base(invocationContext)
        {
            Client = new GoogleDriveClient(InvocationContext.AuthenticationCredentialsProviders);
        }
    }
}
