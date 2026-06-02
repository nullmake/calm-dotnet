using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms.Infrastructure.IO;

/// <summary>
/// A stream that generates random characters.
/// </summary>
/// <param name="length">The random characters length.</param>
internal sealed class RandomCharactorStream(long length) : Stream
{
    /// <summary>
    /// The current position within the stream.
    /// </summary>
    private long _position;

    /// <summary>
    /// A pseudo-random number generator
    /// </summary>
    private readonly Random _random = new();

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => true;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length { get; } = length;

    /// <inheritdoc/>
    public override long Position
    {
        get => _position;
        set => _position = GetPosition(value, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    public override void Flush()
        => throw new NotSupportedException();

    /// <inheritdoc/>
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness",
        Justification = "The weak pseudo-random numbers aren't used in a security-sensitive manner.")]
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        long remaining = Length - _position;
        if (remaining <= 0)
        {
            return 0;
        }

        var bytesToRead = (int)(count < remaining ? count : remaining);

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (var i = 0; i < bytesToRead; i++)
        {
            buffer[offset + i] = (byte)chars[_random.Next(chars.Length)];
        }

        _position += bytesToRead;
        return bytesToRead;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        _position = GetPosition(offset, origin);
        return _position;
    }

    /// <summary>
    /// Gets a position within the current stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/>
    /// indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="origin"/> is invalid.</exception>
    private long GetPosition(long offset, SeekOrigin origin)
    {
        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        if (newPosition < 0)
        {
            newPosition = 0;
        }
        if (Length < newPosition)
        {
            newPosition = Length;
        }
        return newPosition;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}
