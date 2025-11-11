using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;
using Google.Apis.DriveActivity.v2.Data;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class FolderPickerDataSourceHandler(InvocationContext invocationContext) : DriveInvocable(invocationContext), IAsyncFileDataSourceItemHandler
    {
        private const string FolderMime = "application/vnd.google-apps.folder";

        private const string MyDriveVirtualId = "v:mydrive";
        private const string SharedDrivesVirtualId = "v:shared";
        private const string MyDriveDisplay = "My Drive";
        private const string SharedDrivesDisplay = "Shared drives";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(FolderContentDataSourceContext context, CancellationToken cancellationToken)
        {
            var folderId = string.IsNullOrEmpty(context.FolderId) ? string.Empty : context.FolderId;

            if (string.IsNullOrEmpty(folderId))
            {
                return new List<FileDataItem>
                {
                    new Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder { Id = MyDriveVirtualId, DisplayName = MyDriveDisplay, IsSelectable = false },
                    new Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder { Id = SharedDrivesVirtualId, DisplayName = SharedDrivesDisplay, IsSelectable = false }
                };
            }

            if (folderId == MyDriveVirtualId)
            {
                var items = await ListItemsInFolderByIdAsync("root", cancellationToken);
                return items
                    .Where(f => string.Equals(f.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase))
                    .Select(ToFolder)
                    .Cast<FileDataItem>()
                    .ToList();
            }

            if (folderId == SharedDrivesVirtualId)
            {
                var drives = await ListSharedDrivesAsync(cancellationToken);
                return drives
                    .Select(d => new Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder
                    {
                        Id = $"d:{d.Id}",
                        DisplayName = d.Name,
                        IsSelectable = false
                    })
                    .Cast<FileDataItem>()
                    .ToList();
            }

            if (folderId.StartsWith("d:", StringComparison.Ordinal))
            {
                var driveId = folderId.Substring(2);
                var items = await ListItemsInSharedDriveRootAsync(driveId, cancellationToken);

                return items
                    .Where(f => string.Equals(f.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase))
                    .Select(ToFolder)
                    .Cast<FileDataItem>()
                    .ToList();
            }

            var children = await ListItemsInFolderByIdAsync(folderId, cancellationToken);
            return children
                .Where(f => string.Equals(f.MimeType, FolderMime, StringComparison.OrdinalIgnoreCase))
                .Select(ToFolder)
                .Cast<FileDataItem>()
                .ToList();
        }

        public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(FolderPathDataSourceContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(context?.FileDataItemId))
            {
                return new List<FolderPathItem>();
            }

            var id = context.FileDataItemId;

            if (id == MyDriveVirtualId)
                return new List<FolderPathItem> { new() { DisplayName = MyDriveDisplay, Id = MyDriveVirtualId } };

            if (id == SharedDrivesVirtualId)
                return new List<FolderPathItem> { new() { DisplayName = SharedDrivesDisplay, Id = SharedDrivesVirtualId } };

            if (id.StartsWith("d:", StringComparison.Ordinal))
            {
                var driveId = id.Substring(2);
                var drive = await GetDriveAsync(driveId, cancellationToken);
                return new List<FolderPathItem>
                {
                    new() { DisplayName = SharedDrivesDisplay, Id = SharedDrivesVirtualId },
                    new() { DisplayName = drive.Name, Id = $"d:{drive.Id}" }
                };
            }

            try
            {
                var current = await GetFileMetadataByIdAsync(id, cancellationToken);

                if (!string.IsNullOrEmpty(current.DriveId))
                {
                    var drive = await GetDriveAsync(current.DriveId, cancellationToken);
                    var path = new List<FolderPathItem>
                    {
                        new() { DisplayName = SharedDrivesDisplay, Id = SharedDrivesVirtualId },
                        new() { DisplayName = drive.Name, Id = $"d:{drive.Id}" }
                    };

                    var parentId = current.Parents?.FirstOrDefault();
                    var stack = new Stack<FolderPathItem>();

                    while (!string.IsNullOrEmpty(parentId) && parentId != drive.Id)
                    {
                        var parent = await GetFileMetadataByIdAsync(parentId!, cancellationToken);
                        stack.Push(new FolderPathItem { DisplayName = parent.Name, Id = parent.Id });
                        parentId = parent.Parents?.FirstOrDefault();
                    }

                    path.AddRange(stack);
                    return path;
                }
                else
                {
                    var path = new List<FolderPathItem>
                    {
                        new() { DisplayName = MyDriveDisplay, Id = MyDriveVirtualId }
                    };

                    var parentId = current.Parents?.FirstOrDefault();
                    var stack = new Stack<FolderPathItem>();

                    while (!string.IsNullOrEmpty(parentId) && parentId != "root")
                    {
                        var parent = await GetFileMetadataByIdAsync(parentId!, cancellationToken);
                        stack.Push(new FolderPathItem { DisplayName = parent.Name, Id = parent.Id });
                        parentId = parent.Parents?.FirstOrDefault();
                    }

                    path.AddRange(stack);
                    return path;
                }
            }
            catch
            {
                return new List<FolderPathItem>();
            }
        }

        private static Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.Folder ToFolder(Google.Apis.Drive.v3.Data.File f) => new()
        {
            Id = f.Id,
            DisplayName = f.Name,
            Date = f.CreatedTime,
            IsSelectable = true
        };

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
                req.Fields = "nextPageToken, files(id, name, mimeType, parents, createdTime, modifiedTime, driveId)";
                req.PageSize = 100;
                req.PageToken = pageToken;

                var resp = await req.ExecuteAsync(ct);
                if (resp.Files is { Count: > 0 }) files.AddRange(resp.Files);
                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.Drive>> ListSharedDrivesAsync(CancellationToken ct)
        {
            var drives = new List<Google.Apis.Drive.v3.Data.Drive>();
            string? pageToken = null;

            do
            {
                var req = Client.Drives.List();
                req.PageSize = 100;
                req.UseDomainAdminAccess = false;
                req.Fields = "nextPageToken, drives(id, name)";
                req.PageToken = pageToken;

                var resp = await req.ExecuteAsync(ct);
                if (resp.Drives is { Count: > 0 }) drives.AddRange(resp.Drives);
                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return drives;
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> ListItemsInSharedDriveRootAsync(string driveId, CancellationToken ct)
        {
            var files = new List<Google.Apis.Drive.v3.Data.File>();
            string? pageToken = null;

            do
            {
                var req = Client.Files.List();
                req.Corpora = "drive";
                req.DriveId = driveId;
                req.Q = $"'{driveId}' in parents and trashed = false";
                req.IncludeItemsFromAllDrives = true;
                req.SupportsAllDrives = true;
                req.Spaces = "drive";
                req.Fields = "nextPageToken, files(id, name, mimeType, parents, createdTime, modifiedTime, driveId)";
                req.PageSize = 100;
                req.PageToken = pageToken;

                var resp = await req.ExecuteAsync(ct);
                if (resp.Files is { Count: > 0 }) files.AddRange(resp.Files);
                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFileMetadataByIdAsync(string fileId, CancellationToken ct)
        {
            var req = Client.Files.Get(fileId);
            req.SupportsAllDrives = true;
            req.Fields = "id, name, mimeType, parents, createdTime, modifiedTime, driveId";
            return await req.ExecuteAsync(ct);
        }

        private async Task<Google.Apis.Drive.v3.Data.Drive> GetDriveAsync(string driveId, CancellationToken ct)
        {
            var req = Client.Drives.Get(driveId);
            req.Fields = "id, name";
            return await req.ExecuteAsync(ct);
        }
    }
}
