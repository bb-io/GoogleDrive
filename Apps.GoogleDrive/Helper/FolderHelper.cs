using Apps.GoogleDrive.Invocables;

namespace Apps.GoogleDrive.Helper;

public static class FolderHelper
{
    private const int ParentIdsPerQuery = 20;

    public static async Task<List<string>> GetAllSubfolderIds(DriveInvocable invocable, string rootId, double? maxLevel)
    {
        var allFolderIds = new List<string>();
        var discoveredFolderIds = new HashSet<string> { rootId };
        var currentLevelFolderIds = new List<string> { rootId };
        var currentLevel = 0;

        while (currentLevelFolderIds.Count > 0 && (!maxLevel.HasValue || currentLevel < maxLevel.Value))
        {
            var nextLevelFolderIds = new List<string>();

            foreach (var parentIdBatch in currentLevelFolderIds.Chunk(ParentIdsPerQuery))
            {
                string? pageToken = null;
                var parentQuery = string.Join(" or ", parentIdBatch.Select(id => $"'{EscapeDriveQueryValue(id)}' in parents"));

                do
                {
                    var request = invocable.Client.Files.List();
                    request.Q = $"({parentQuery}) and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
                    request.IncludeItemsFromAllDrives = true;
                    request.SupportsAllDrives = true;
                    request.Spaces = "drive";
                    request.Fields = "nextPageToken, files(id)";
                    request.PageSize = 1000;
                    request.PageToken = pageToken;

                    var response = await invocable.ExecuteWithErrorHandlingAsync(() => request.ExecuteAsync());

                    if (response.Files != null)
                    {
                        foreach (var folder in response.Files.Where(folder => discoveredFolderIds.Add(folder.Id)))
                        {
                            allFolderIds.Add(folder.Id);
                            nextLevelFolderIds.Add(folder.Id);
                        }
                    }

                    pageToken = response.NextPageToken;
                } while (!string.IsNullOrEmpty(pageToken));
            }

            currentLevelFolderIds = nextLevelFolderIds;
            currentLevel++;
        }

        return allFolderIds;
    }

    private static string EscapeDriveQueryValue(string value) => value.Replace("\\", "\\\\").Replace("'", "\\'");
}
