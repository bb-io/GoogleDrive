using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.Actions;

[ActionList]
public class DebugActions(InvocationContext invocationContext) : DriveInvocable(invocationContext)
{
    [Action("[Debug] Action", Description = "Debug action")]
    public List<AuthenticationCredentialsProvider> DebugAction() =>
        InvocationContext.AuthenticationCredentialsProviders.ToList();
}