using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.GoogleDrive.Utils.StreamWrappers;

/// <summary>
/// Memory stream enforcing a maximum size (used for Google Docs exports).
/// </summary>
public class LimitedMemoryStream : MemoryStream
{
    private const long GoogleDocsExportLimitBytes = 50L * 1024 * 1024; // 50 MB, more than the documented 10 MB limit to be safe

    private void EnsureLimit(int incoming)
    {
        if (Length + incoming > GoogleDocsExportLimitBytes)
            throw new PluginApplicationException($"Google Docs export exceeds 80 MB limit.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureLimit(count);
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureLimit(buffer.Length);
        base.Write(buffer);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureLimit(buffer.Length);
        return base.WriteAsync(buffer, cancellationToken);
    }

    public override void WriteByte(byte value)
    {
        EnsureLimit(1);
        base.WriteByte(value);
    }
}
