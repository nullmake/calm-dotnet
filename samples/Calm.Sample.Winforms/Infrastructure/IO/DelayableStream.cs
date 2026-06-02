using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Calm.Sample.Winforms.Infrastructure.IO;

/// <summary>
/// Delayable <see cref="DelayableStream"/>
/// </summary>
/// <param name="stream">The original <see cref="Stream"/> instance.</param>
/// <param name="delay">The amount of the delay.</param>
internal sealed class DelayableStream(Stream stream, TimeSpan delay) : Stream
{
    /// <summary>
    /// The <see cref="Subject"/> of the flush event.
    /// </summary>
    private readonly Subject<Stream> _flushEvent = new();

    /// <summary>
    /// The flush event stream.
    /// </summary>
    public IObservable<Stream> FlushEvent => _flushEvent.AsObservable();

    /// <summary>
    /// The <see cref="Subject"/> of the read event.
    /// </summary>
    private readonly Subject<Stream> _readEvent = new();

    /// <summary>
    /// The read event stream.
    /// </summary>
    public IObservable<Stream> ReadEvent => _readEvent.AsObservable();

    /// <summary>
    /// The <see cref="Subject"/> of the write event.
    /// </summary>
    private readonly Subject<Stream> _writeEvent = new();

    /// <summary>
    /// The read event stream.
    /// </summary>
    public IObservable<Stream> WriteEvent => _writeEvent.AsObservable();

    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _flushEvent.OnCompleted();
                _flushEvent.Dispose();
                _readEvent.OnCompleted();
                _readEvent.Dispose();
                _writeEvent.OnCompleted();
                _writeEvent.Dispose();
                stream.Dispose();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _readEvent.OnCompleted();
            _readEvent.Dispose();
            _writeEvent.OnCompleted();
            _writeEvent.Dispose();
            await stream.DisposeAsync().ConfigureAwait(false);
        }
        _disposed = true;
        await base.DisposeAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override bool CanRead => stream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => stream.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => stream.CanWrite;

    /// <inheritdoc/>
    public override long Length => stream.Length;

    /// <inheritdoc/>
    public override long Position { get => stream.Position; set => stream.Position = value; }

    /// <inheritdoc/>
    public override void Flush()
    {
        _flushEvent.OnNext(this);
        stream.Flush();
    }

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        _flushEvent.OnNext(this);
        return stream.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        Task.Delay(delay).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        var size = stream.Read(buffer, offset, count);
        _readEvent.OnNext(this);
        return size;
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        var size = await stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        _readEvent.OnNext(this);
        return size;
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        var size = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        _readEvent.OnNext(this);
        return size;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

    /// <inheritdoc/>
    public override void SetLength(long value) => stream.SetLength(value);

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        Task.Delay(delay).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        stream.Write(buffer, offset, count);
        _writeEvent.OnNext(this);
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
        _writeEvent.OnNext(this);
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        _writeEvent.OnNext(this);
    }
}
