using Apps.GoogleDrive.Invocables;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems;

namespace Apps.GoogleDrive.DataSourceHandler
{
    public class FolderPickerDataSourceHandler(InvocationContext invocationContext) : DriveInvocable(invocationContext), IAsyncFileDataSourceItemHandler
    {
        private const string VMyDrive = "__VIRTUAL_MY_DRIVE__";
        private const string VShared = "__VIRTUAL_SHARED__";

        private const string MyDriveDisplay = "My Drive";
        private const string SharedDisplay = "Shared Drives";

        private const string FolderMime = "application/vnd.google-apps.folder";
        private const string ShortcutMime = "application/vnd.google-apps.shortcut";

        public async Task<IEnumerable<FileDataItem>> GetFolderContentAsync(
            FolderContentDataSourceContext context, CancellationToken ct)
        {
            var folderId = context?.FolderId;

            if (string.IsNullOrEmpty(folderId))
            {
                return new FileDataItem[]
                {
                    new Folder { Id = VMyDrive, DisplayName = MyDriveDisplay, IsSelectable = false },
                    new Folder { Id = VShared,  DisplayName = SharedDisplay,  IsSelectable = false }
                };
            }

            if (folderId == VMyDrive)
            {
                var items = await ListPagedAsync(
                    q: "trashed = false",
                    corpora: "user",
                    ct: ct);

                return MapFilesAndFolders(items);
            }

            if (folderId == VShared)
            {
                var items = await ListPagedAsync(
                    q: "trashed = false AND sharedWithMe = true",
                    corpora: "user",
                    ct: ct);

                return MapFilesAndFolders(items);
            }

            var children = await ListPagedAsync(
                q: $"'{folderId}' in parents and trashed = false",
                corpora: "allDrives",
                ct: ct);

            return MapFilesAndFolders(children);
        }

        public async Task<IEnumerable<FolderPathItem>> GetFolderPathAsync(
            FolderPathDataSourceContext context, CancellationToken ct)
        {
            var id = context?.FileDataItemId;
            if (string.IsNullOrEmpty(id))
                return new[] { new FolderPathItem { DisplayName = MyDriveDisplay, Id = VMyDrive } };

            if (id == VMyDrive) return new[] { new FolderPathItem { DisplayName = MyDriveDisplay, Id = VMyDrive } };
            if (id == VShared) return new[] { new FolderPathItem { DisplayName = SharedDisplay, Id = VShared } };

            var result = new List<FolderPathItem>();
            try
            {
                var current = await GetFileMetaAsync(id, ct);

                var isMine = current.OwnedByMe == true;
                var isShared = current.Shared == true && !isMine;

                result.Add(new FolderPathItem
                {
                    DisplayName = isMine ? MyDriveDisplay : SharedDisplay,
                    Id = isMine ? VMyDrive : VShared
                });

                var stack = new Stack<FolderPathItem>();
                var parentId = current.Parents?.FirstOrDefault();

                while (!string.IsNullOrEmpty(parentId))
                {
                    var parent = await GetFileMetaAsync(parentId!, ct);
                    stack.Push(new FolderPathItem { DisplayName = parent.Name, Id = parent.Id });
                    parentId = parent.Parents?.FirstOrDefault();
                }

                while (stack.Count > 0) result.Add(stack.Pop());
            }
            catch
            {
                result.Clear();
                result.Add(new FolderPathItem { DisplayName = MyDriveDisplay, Id = VMyDrive });
            }

            return result;
        }

        private IEnumerable<FileDataItem> MapFilesAndFolders(IEnumerable<Google.Apis.Drive.v3.Data.File> items)
        {
            var list = new List<FileDataItem>();
            foreach (var f in items)
            {
                var (id, mime) = ResolveShortcut(f);
                var isFolder = string.Equals(mime, FolderMime, StringComparison.OrdinalIgnoreCase);

                if (isFolder)
                {
                    list.Add(new Folder
                    {
                        Id = id,
                        DisplayName = f.Name,
                        Date = f.CreatedTime,
                        IsSelectable = false
                    });
                }
                else
                {
                    list.Add(new Blackbird.Applications.SDK.Extensions.FileManagement.Models.FileDataSourceItems.File
                    {
                        Id = id,
                        DisplayName = f.Name,
                        Date = f.ModifiedTime ?? f.CreatedTime,
                        Size = f.Size,
                        IsSelectable = true
                    });
                }
            }
            return list;
        }

        private static (string id, string mime) ResolveShortcut(Google.Apis.Drive.v3.Data.File f)
        {
            if (string.Equals(f.MimeType, ShortcutMime, StringComparison.OrdinalIgnoreCase) &&
                f.ShortcutDetails?.TargetId is { } tid &&
                !string.IsNullOrEmpty(f.ShortcutDetails.TargetMimeType))
            {
                return (tid, f.ShortcutDetails.TargetMimeType);
            }
            return (f.Id, f.MimeType);
        }

        private async Task<IList<Google.Apis.Drive.v3.Data.File>> ListPagedAsync(string q, string corpora, CancellationToken ct)
        {
            var files = new List<Google.Apis.Drive.v3.Data.File>();
            string? pageToken = null;

            do
            {
                var req = Client.Files.List();
                req.Q = q;
                req.Corpora = corpora;
                req.IncludeItemsFromAllDrives = true;
                req.SupportsAllDrives = true;
                req.Spaces = "drive";
                req.PageSize = 100;
                req.PageToken = pageToken;
                req.Fields =
                    "nextPageToken, files(" +
                    "id, name, mimeType, size, parents, createdTime, modifiedTime, " +
                    "ownedByMe, shared, driveId, " +
                    "shortcutDetails(targetId,targetMimeType)" +
                    ")";

                var resp = await req.ExecuteAsync(ct);
                if (resp.Files is { Count: > 0 })
                    files.AddRange(resp.Files);

                pageToken = resp.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return files;
        }

        private async Task<Google.Apis.Drive.v3.Data.File> GetFileMetaAsync(string id, CancellationToken ct)
        {
            var req = Client.Files.Get(id);
            req.SupportsAllDrives = true;
            req.Fields =
                "id, name, mimeType, size, parents, createdTime, modifiedTime, " +
                "ownedByMe, shared, driveId, " +
                "shortcutDetails(targetId,targetMimeType)";
            return await req.ExecuteAsync(ct);
        }
    }
}
