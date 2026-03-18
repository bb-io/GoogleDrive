using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Storage.Responses;
using Apps.GoogleDrive.Polling.Models;
using Apps.GoogleDrive.Polling.Models.Memory;
using Apps.GoogleDrive.Utils;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using File = Google.Apis.Drive.v3.Data.File;
using FileInfo = Apps.GoogleDrive.Models.Storage.Responses.FileInfo;

namespace Apps.GoogleDrive.Polling;

[PollingEventList("Files")]
public class PollingList(InvocationContext invocationContext) : DriveInvocable(invocationContext)
{
    [PollingEvent("On files deleted in shared drives", "On files deleted in shared drives")]
    public Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFilesDeleted(
        PollingEventRequest<DateMemory> request) => HandleFilesPolling(request,
        x => x.TrashedTimeDateTimeOffset?.UtcDateTime > request.Memory?.LastInteractionDate);

    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files created", "On files created in a specified folder")]
    public async Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFileCreated(
        PollingEventRequest<DateMemory> request,
        [PollingEventParameter] OnFileCreatedRequest filter)
    {
        if (request.Memory is null) return CreateEmptyResponse();

        var items = await GetFilesWithSubfoldersAsync(
            request.Memory.LastInteractionDate,
            filter.FolderId,
            filter.IncludeSubfolders,
            filter.MaxSubfolderLevel,
            filter.FileNameContains,
            filter.MimeType,
            "createdTime"
        );

        return items.Count != 0 ? CreateSuccessResponse(items) : CreateEmptyResponse();
    }

    [PollingEvent("On files updated", "On files updated in a specified folder")]
    public async Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFileUpdated(
        [PollingEventParameter] OnFileUpdateRequest filter,
        PollingEventRequest<DateMemory> request)
    {
        if (request.Memory is null) return CreateEmptyResponse();

        var items = await GetFilesWithSubfoldersAsync(
            request.Memory.LastInteractionDate,
            filter.FolderId,
            filter.IncludeSubfolders,
            filter.MaxSubfolderLevel, 
            fileNameContains: null,
            mimeTypeFilter: null,
            dateField: "modifiedTime"
        );

        if (!string.IsNullOrEmpty(filter.FileId))
            items = items.Where(x => x.Id == filter.FileId).ToList();

        return items.Count != 0 ? CreateSuccessResponse(items) : CreateEmptyResponse();
    }

    private async Task<PollingEventResponse<DateMemory, SearchFilesResponse>> HandleFilesPolling(
        PollingEventRequest<DateMemory> request, Func<File, bool> filter)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        var items = (await GetAllFilesAsync())
            .Where(filter)
            .ToArray();

        if (!items.Any())
        {
            return new()
            {
                FlyBird = false,
                Memory = new()
                {
                    LastInteractionDate = DateTime.UtcNow
                }
            };
        }

        return new()
        {
            FlyBird = true,
            Result = new()
            {
                Files = items.Select(x => new FileInfo(x)).ToList(),
                TotalCount = items.Length
            },
            Memory = new()
            {
                LastInteractionDate = DateTime.UtcNow
            }
        };
    }

    private async Task<List<File>> GetFilesWithSubfoldersAsync(
        DateTime lastInteractionDate, 
        string? folderId, 
        bool? includeSubfolders,
        double? maxLevel, 
        string? fileNameContains, 
        string? mimeTypeFilter, 
        string dateField)
    {
        var lastInteractionIso = lastInteractionDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm':'ss.fff'Z'");
        var folderIds = new List<string>();

        if (!string.IsNullOrEmpty(folderId))
        {
            folderIds.Add(folderId);

            if (includeSubfolders == true)
            {
                var subfolders = await GetAllSubfolderIdsAsync(folderId, maxLevel);
                folderIds.AddRange(subfolders);
            }
        }

        var queryParts = new List<string>
        {
            $"{dateField} > '{lastInteractionIso}'",
            "trashed = false",
            "mimeType != 'application/vnd.google-apps.folder'"
        };

        if (folderIds.Count != 0)
        {
            var parentQueries = folderIds.Select(id => $"'{EscapeDriveQueryValue(id)}' in parents");
            queryParts.Add($"({string.Join(" or ", parentQueries)})");
        }

        if (!string.IsNullOrWhiteSpace(fileNameContains))
            queryParts.Add($"name contains '{EscapeDriveQueryValue(fileNameContains.Trim())}'");

        if (!string.IsNullOrWhiteSpace(mimeTypeFilter))
            queryParts.Add($"mimeType = '{EscapeDriveQueryValue(mimeTypeFilter.Trim())}'");

        return await SearchFilesAsync(string.Join(" and ", queryParts));
    }

    private async Task<List<string>> GetAllSubfolderIdsAsync(string rootId, double? maxLevel)
    {
        var allFolderIds = new List<string>();
        var foldersToProcess = new Queue<(string Id, int Level)>();
        foldersToProcess.Enqueue((rootId, 0));

        while (foldersToProcess.Count > 0)
        {
            var (currentId, currentLevel) = foldersToProcess.Dequeue();

            if (maxLevel.HasValue && currentLevel >= maxLevel.Value) continue;

            var request = Client.Files.List();
            request.Q = $"'{currentId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            request.Fields = "nextPageToken, files(id)";

            var response = await ExecuteWithErrorHandlingAsync(() => request.ExecuteAsync(CancellationToken.None));

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

    private static string EscapeDriveQueryValue(string value) => value.Replace("\\", "\\\\").Replace("'", "\\'");

    private async Task<List<File>> SearchFilesAsync(string query)
    {
        var allFiles = new List<File>();
        var pageToken = (string)null!;

        do
        {
            var request = Client.Files.List();
            request.IncludeItemsFromAllDrives = true;
            request.SupportsAllDrives = true;
            request.Fields = "nextPageToken, files(id, name, parents, createdTime, trashedTime, trashed, modifiedTime, mimeType, size)";
            request.PageSize = 100;
            request.PageToken = pageToken;
            request.Q = query;

            var result = await ExecuteWithErrorHandlingAsync(() => RetryHandler.ExecuteAsync(() => request.ExecuteAsync(CancellationToken.None), options: null, ct: CancellationToken.None));

            if (result.Files is { Count: > 0 })
                allFiles.AddRange(result.Files);

            pageToken = result.NextPageToken;

        } while (pageToken != null);

        return allFiles;
    }

    private async Task<List<File>> GetAllFilesAsync()
    {
        var allFiles = new List<File>();
        var pageToken = (string)null!;

        do
        {
            var request = Client.Files.List();
            request.IncludeItemsFromAllDrives = true;
            request.SupportsAllDrives = true;
            request.Fields = "nextPageToken, files(id, name, parents, createdTime, trashedTime, trashed, modifiedTime, mimeType, size)";
            request.PageSize = 100;
            request.PageToken = pageToken;

            var result = await ExecuteWithErrorHandlingAsync(() =>
           RetryHandler.ExecuteAsync(() => request.ExecuteAsync(CancellationToken.None), options: null, ct: CancellationToken.None));
            if (result.Files is { Count: > 0 })
                allFiles.AddRange(result.Files);

            pageToken = result.NextPageToken;
        } while (pageToken != null);

        return allFiles;
    }

    private static PollingEventResponse<DateMemory, SearchFilesResponse> CreateEmptyResponse() => new()
    {
        FlyBird = false,
        Memory = new() { LastInteractionDate = DateTime.UtcNow }
    };

    private static PollingEventResponse<DateMemory, SearchFilesResponse> CreateSuccessResponse(IEnumerable<File> items)
    {
        var fileArray = items.ToArray();
        return new()
        {
            FlyBird = true,
            Result = new()
            {
                Files = fileArray.Select(x => new FileInfo(x)).ToList(),
                TotalCount = fileArray.Length
            },
            Memory = new() { LastInteractionDate = DateTime.UtcNow }
        };
    }
}
