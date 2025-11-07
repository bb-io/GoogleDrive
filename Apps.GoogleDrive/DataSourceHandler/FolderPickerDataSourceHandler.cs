using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class FolderPickerDataSourceHandler(InvocationContext invocationContext) : DriveInvocable(invocationContext), IAsyncFileDataSourceItemHandler
    {
        private const string RootFolderDisplayName = "My Drive";
        private const string FolderMime = "application/vnd.google-apps.folder";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context, CancellationToken cancellationToken)
        {
            var folderId = string.IsNullOrEmpty(context.FolderId) ? "root" : context.FolderId;

            if (folderId == "root")
            {
                var myDriveChildren = await ListItemsInFolderByIdAsync("root", cancellationToken);
                var sharedFolders = await ListSharedWithMeFoldersAsync(cancellationToken);

                var myDriveFolders = myDriveChildren
                    .Where(f => string.Equals(f.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase));

                var byId = new Dictionary<string, Google.Apis.Drive.v3.Data.File>();
                foreach (var f in myDriveFolders) byId[f.Id] = f;
                foreach (var f in sharedFolders) byId[f.Id] = f;

                return byId.Values
                    .Select(f => new Folder
                    {
                        Id = f.Id,
                        DisplayName = f.Name,
                        Date = f.CreatedTime,
                        IsSelectable = true
                    })
                    .Cast<FileDataItem>()
                    .ToList();
            }

            var items = await ListItemsInFolderByIdAsync(folderId, cancellationToken);

            var onlyFolders = items
                .Where(f => string.Equals(f.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase))
                .Select(f => new Folder
                {
                    Id = f.Id,
                    DisplayName = f.Name,
                    Date = f.CreatedTime,
                    IsSelectable = true
                })
                .Cast<FileDataItem>()
                .ToList();

            return onlyFolders;
        }

        public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(context?.FileDataItemId))
            {
                return new List<FolderPathItem>
                {
                    new() { DisplayName = RootFolderDisplayName, Id = "root" }
                };
            }

            var result = new List<FolderPathItem>();

            try
            {
                var current = await GetFileMetadataByIdAsync(context.FileDataItemId!, cancellationToken);
                var parentId = current.Parents?.FirstOrDefault();

                while (!string.IsNullOrEmpty(parentId))
                {
                    var parent = await GetFileMetadataByIdAsync(parentId!, cancellationToken);

                    result.Insert(0, new FolderPathItem
                    {
                        DisplayName = parent.Name,
                        Id = parent.Id
                    });

                    if (string.Equals(parentId, "root", StringComparison.Ordinal))
                        break;

                    parentId = parent.Parents?.FirstOrDefault();
                }

                if (result.Count == 0 || !string.Equals(result[0].Id, "root", StringComparison.Ordinal))
                {
                    result.Insert(0, new FolderPathItem { DisplayName = RootFolderDisplayName, Id = "root" });
                }
                else
                {
                    result[0].DisplayName = RootFolderDisplayName;
                    result[0].Id = "root";
                }
            }
            catch
            {
                result.Clear();
                result.Add(new FolderPathItem { DisplayName = RootFolderDisplayName, Id = "root" });
            }

            return result;
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> ListItemsInFolderByIdAsync(string folderId, CancellationToken ct)
        {
            var files = new List<Google.Apis.Drive.v3.Data.File>();
            string? pageToken = null;

            do
            {
                var req = Client.Files.List();
                req.Q = $"'{folderId}' in parents and trashed = false";
                req.IncludeItemsFromAllDrives = true;
                req.SupportsAllDrives = true;
                req.Spaces = "drive";
                req.Fields = "nextPageToken, files(id, name, mimeType, size, parents, createdTime, modifiedTime)";
                req.PageSize = 100;
                req.PageToken = pageToken;

                var resp = await req.ExecuteAsync(ct);

                if (resp.Files is { Count: > 0 })
                    files.AddRange(resp.Files);

                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> ListSharedWithMeFoldersAsync(CancellationToken ct)
        {
            var files = new List<Google.Apis.Drive.v3.Data.File>();
            string? pageToken = null;

            do
            {
                var req = Client.Files.List();
                req.Q = $"sharedWithMe = true and trashed = false and mimeType = '{FolderMime}'";
                req.IncludeItemsFromAllDrives = true;
                req.SupportsAllDrives = true;
                req.Spaces = "drive";
                req.Fields = "nextPageToken, files(id, name, mimeType, size, parents, createdTime, modifiedTime)";
                req.PageSize = 100;
                req.PageToken = pageToken;

                var resp = await req.ExecuteAsync(ct);

                if (resp.Files is { Count: > 0 })
                    files.AddRange(resp.Files);

                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFileMetadataByIdAsync(string fileId, CancellationToken ct)
        {
            var req = Client.Files.Get(fileId);
            req.SupportsAllDrives = true;
            req.Fields = "id, name, mimeType, size, parents, createdTime, modifiedTime";
            return await req.ExecuteAsync(ct);
        }
    }
}
