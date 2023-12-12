using Apps.GoogleDrive.Actions;
using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Dynamic;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.GoogleDrive.DataSourceHandler;

public class FolderDataHandler : DriveInvocable, IDataSourceHandler
{
    public FolderDataHandler(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    public Dictionary<string, string> GetData(DataSourceContext context)
    {
        var query = "mimeType = 'application/vnd.google-apps.folder'";
        if (context.SearchString != null)
            query += $" and name contains '{context.SearchString}'";

        var filesListr = Client.Files.List();

        filesListr.SupportsAllDrives = true;
        filesListr.Q = query;
        filesListr.PageSize = 20;
        
        var filesList = filesListr.Execute();

        return filesList.Files.ToDictionary(x => x.Id, x => x.Name);
    }
}