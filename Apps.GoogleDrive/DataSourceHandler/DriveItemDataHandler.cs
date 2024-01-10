using Apps.GoogleDrive.Actions;
using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.DataSourceHandler;

public class DriveItemDataHandler : DriveInvocable, IDataSourceHandler
{

    public DriveItemDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public Dictionary<string, string> GetData(DataSourceContext context)
    {
        var filesListr = Client.Files.List();

        filesListr.SupportsAllDrives = true;
        if (context.SearchString != null)
            filesListr.Q += $"name contains '{context.SearchString}'";
        filesListr.PageSize = 20;

        var filesList = filesListr.Execute();

        return filesList.Files.ToDictionary(x => x.Id, x => x.Name);
    }
}