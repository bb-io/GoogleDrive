using Apps.GoogleDrive.Invocables;
using Apps.GoogleDrive.Models.Storage.Responses;
using Apps.GoogleDrive.Polling.Models;
using Apps.GoogleDrive.Polling.Models.Memory;
using Apps.GoogleDrive.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.Sdk.Common.Polling;
using Blackbird.Applications.SDK.Blueprints;
using File = Google.Apis.Drive.v3.Data.File;
using FileInfo = Apps.GoogleDrive.Models.Storage.Responses.FileInfo;

namespace Apps.GoogleDrive.Polling;

[PollingEventList]
public class PollingList : DriveInvocable
{
    public PollingList(InvocationContext invocationContext) : base(invocationContext)
    {
    }

    [PollingEvent("On files deleted in shared drives", "On any files are deleted in shared drives")]
    public Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFilesDeleted(
        PollingEventRequest<DateMemory> request) => HandleFilesPolling(request,
        x => x.TrashedTimeDateTimeOffset?.UtcDateTime > request.Memory?.LastInteractionDate);

    [BlueprintEventDefinition(BlueprintEvent.FilesCreatedOrUpdated)]
    [PollingEvent("On files created", "On any file created in specified folder")]
    public async Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFileCreated(PollingEventRequest<DateMemory> request,
        [PollingEventParameter]OnFileCreatedRequest filter)
    {
        return await HandleCreatedFilesPolling(request, filter.FolderId);
    }

    [PollingEvent("On files updated", "On any file updated in specified folder")]
    public Task<PollingEventResponse<DateMemory, SearchFilesResponse>> OnFileUpdated(
        [PollingEventParameter] OnFileUpdateRequest filter,
       PollingEventRequest<DateMemory> request) => HandleFilesPolling(request,
       x => x.ModifiedTimeDateTimeOffset?.UtcDateTime > request.Memory?.LastInteractionDate
        && (string.IsNullOrEmpty(filter.FileId) || x.Id == filter.FileId)
        && (string.IsNullOrEmpty(filter.FolderId)|| x.Parents?.Contains(filter.FolderId) == true));


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

    private async Task<PollingEventResponse<DateMemory, SearchFilesResponse>> HandleCreatedFilesPolling(
    PollingEventRequest<DateMemory> request,
    string folderId)
    {
        if (request.Memory is null)
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
            };
        }

        var lastInteractionIso = request.Memory.LastInteractionDate
             .ToUniversalTime()
             .ToString("yyyy-MM-dd'T'HH':'mm':'ss.fff'Z'");
        
        var query = $"createdTime > '{lastInteractionIso}' and '{folderId}' in parents and trashed = false and mimeType != 'application/vnd.google-apps.folder'";
        var items = (await SearchFilesAsync(query))
            .ToArray();

        if (!items.Any())
        {
            return new()
            {
                FlyBird = false,
                Memory = new() { LastInteractionDate = DateTime.UtcNow }
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
            Memory = new() { LastInteractionDate = DateTime.UtcNow }
        };
    }

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
}