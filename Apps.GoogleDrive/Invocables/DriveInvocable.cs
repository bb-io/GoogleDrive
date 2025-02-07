using Apps.GoogleDrive.Clients;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Google;

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

        protected async Task<T> ExecuteSafeAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (GoogleApiException gEx)
            {
                if (gEx.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new PluginApplicationException($"The file or folder was not found. Please check your input and try again");
                }

                throw new PluginApplicationException($"Error while making a request to Google API: {gEx.Message}");
            }
            catch (Exception ex)
            {
                throw new PluginApplicationException(ex.Message);
            }
        }
    }
}
