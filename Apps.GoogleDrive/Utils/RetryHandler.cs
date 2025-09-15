using Google;
using System.Net;

namespace Apps.GoogleDrive.Utils
{
    public static class RetryHandler
    {
        private static readonly HashSet<HttpStatusCode> RetryableStatusCodes = new()
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)429
        };

        private static bool ShouldRetry(Exception ex)
        {
            if (ex is TaskCanceledException || ex is HttpRequestException)
                return true;

            if (ex is GoogleApiException gEx)
            {
                var reason = gEx.Error?.Errors?.FirstOrDefault()?.Reason;
                if (!string.IsNullOrWhiteSpace(reason) &&
                    (reason is "backendError" or "internalError" or "rateLimitExceeded" or "userRateLimitExceeded"))
                    return true;

                if (RetryableStatusCodes.Contains(gEx.HttpStatusCode))
                    return true;
            }

            return false;
        }

        private static TimeSpan ComputeBackoff(int attempt, GoogleApiRetryOptions o)
        {
            var jitter = Random.Shared.Next(0, o.JitterMs);
            var backoff = Math.Min(o.BaseDelayMs * Math.Pow(2, attempt - 1), o.MaxDelayMs);
            return TimeSpan.FromMilliseconds(backoff + jitter);
        }

        public static async Task<T> ExecuteAsync<T>(
       Func<Task<T>> action,
       GoogleApiRetryOptions? options = null,
       CancellationToken ct = default)
        {
            var o = options ?? new GoogleApiRetryOptions();

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (attempt < o.MaxAttempts && ShouldRetry(ex))
                {
                    await Task.Delay(ComputeBackoff(attempt, o), ct);
                    continue;
                }
            }
        }

        public static Task ExecuteAsync(
            Func<Task> action,
            GoogleApiRetryOptions? options = null,
            CancellationToken ct = default)
            => ExecuteAsync<object>(async () => { await action(); return default!; }, options, ct);
    }
    public class GoogleApiRetryOptions
    {
        public int MaxAttempts { get; init; } = 5;
        public int BaseDelayMs { get; init; } = 500;
        public int MaxDelayMs { get; init; } = 8000;
        public int JitterMs { get; init; } = 250;
    }
}
