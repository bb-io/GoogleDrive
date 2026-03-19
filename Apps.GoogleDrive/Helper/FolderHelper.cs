using Apps.GoogleDrive.Invocables;

namespace Apps.GoogleDrive.Helper;

public static class FolderHelper
{
    public static async Task<List<string>> GetAllSubfolderIds(DriveInvocable invocable, string rootId, double? maxLevel)
    {
        var allFolderIds = new List<string>();
        var foldersToProcess = new Queue<(string Id, int Level)>();
        foldersToProcess.Enqueue((rootId, 0));

        while (foldersToProcess.Count > 0)
        {
            var (currentId, currentLevel) = foldersToProcess.Dequeue();

            if (maxLevel.HasValue && currentLevel >= maxLevel.Value)
                continue;

            var request = invocable.Client.Files.List();
            request.Q = $"'{currentId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            request.Fields = "nextPageToken, files(id)";

            var response = await invocable.ExecuteWithErrorHandlingAsync(async () => await request.ExecuteAsync());

            if (response.Files != null)
            {
                foreach (var folder in response.Files)
                {
                    allFolderIds.Add(folder.Id);
                    foldersToProcess.Enqueue((folder.Id, currentLevel + 1));
                }
            }
        }
        return allFolderIds;
    }
}
