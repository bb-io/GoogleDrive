namespace Apps.GoogleDrive.Utils.StreamWrappers;

/// <summary>
/// Stream wrapper that supplies a known Length while streaming (non-seekable).
/// Used for memory efficient downloading large Google Drive files where the length is known from metadata.
/// </summary>
public class KnownLengthForwardingStream : Stream
{
    private readonly Stream _inner;
    private readonly long _length;
    private long _position;

    public KnownLengthForwardingStream(Stream inner, long length)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _length = length;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _inner.Read(buffer, offset, count);
        _position += read;
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await _inner.ReadAsync(buffer, cancellationToken);
        _position += read;
        return read;
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => base.ReadAsync(buffer, offset, count, cancellationToken);

    public override void Flush() { }
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _inner.Dispose();
        base.Dispose(disposing);
    }
}
