using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.IO;
using Calm.Sample.Winforms.Models.Bus.Commands;
using Calm.Sample.Winforms.Models.Bus.Events;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Calm.Sample.Winforms.Models.Services.Compression;

/// <summary>
/// Optimize the compression.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="calm">The calm engine instance.</param>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed class RecompressorService(ILogger<RecompressorService> logger, ICalm calm) : IDisposable
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The calm engine instance.
    /// </summary>
    private readonly ICalm _calm = calm;

    #region IDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing '{Class}' instance.", nameof(RecompressorService));
        _calm.Unregister(this);
        _disposed = true;
    }
    #endregion

    /// <summary>
    /// Handles the <see cref="RecompressCommand"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private async Task HandleRecompressCommandAsync(RecompressCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        _logger.LogInformation("Handle comand: {Command}", command);

        const string pattern = "*.zip";
        var option = command.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        await Task.WhenAll(Directory.EnumerateFiles(command.FolderPath, pattern, option)
            .Select(archive => RecompressArchiveAsync(archive, command.Delay, token)));
    }

    /// <summary>
    /// Recompress the specified archive.
    /// </summary>
    /// <param name="path">The archive file path.</param>
    /// <param name="delay">The wait time immediately before reading the stream.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Errors are forwarded to the observer")]
    private async Task RecompressArchiveAsync(string path, TimeSpan delay, CancellationToken token)
    {
        var originalSize = new FileInfo(path).Length;
        var tempFilePath = path + ".tmp";
        try
        {
            var startEvent = new RecompressProgressEvent(path, originalSize, 0, "Start");
            await _calm.Event.PublishAsync(startEvent, token).ConfigureAwait(true);

            // As this section does not require thread safety, it uses ConfigureAwait(false).
#pragma warning disable CA2000 // Dispose objects before losing scope
            var source = new DelayableStream(File.OpenRead(path), delay);
#pragma warning restore CA2000 // Dispose objects before losing scope
            source.ReadEvent.Subscribe(s =>
            {
                var rate = string.Create(CultureInfo.InvariantCulture, $"{s.Position * 100 / s.Length}%");
                _calm.Event.Publish(new RecompressProgressEvent(path, originalSize, 0, rate), token);
            }, CancellationToken.None);

            await using (source.ConfigureAwait(false))
            {
                var destination = File.Create(tempFilePath);
                await using (destination.ConfigureAwait(false))
                {
                    await ProcessRecompressionInternalAsync(source, destination, token)
                        .ConfigureAwait(false);
                }
            }
            File.Delete(path);
            File.Move(tempFilePath, path);

            // Return to the Calm Engine thread.
            // This is not strictly necessary since this method returns to the Calm Engine thread.
            await _calm.SwitchAsync();

            var endEvent = new RecompressProgressEvent(path, originalSize, new FileInfo(path).Length, "Recompressed");
            await _calm.Event.PublishAsync(endEvent, token).ConfigureAwait(true);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "{Message}", ex.Message);
        }
        catch (Exception ex)
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
            var errorEvent = new RecompressProgressEvent(path, originalSize, originalSize, $"Error: {ex.Message}");
            await _calm.Event.PublishAsync(errorEvent, token).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Internal method to process re-compression.
    /// </summary>
    /// <param name="source">The source stream.</param>
    /// <param name="destination">The destination stream.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task ProcessRecompressionInternalAsync(Stream source, Stream destination, CancellationToken ct)
    {
        var readerOptions = new ReaderOptions
        {
            LeaveStreamOpen = true
        };
        var writerOptions = new WriterOptions(CompressionType.Deflate, 9)
        {
            LeaveStreamOpen = true
        };

        var reader = await ReaderFactory.OpenAsyncReader(source, readerOptions, ct)
            .ConfigureAwait(false);
        await using (reader.ConfigureAwait(false))
        {
            var writer = await WriterFactory.OpenAsyncWriter(destination, ArchiveType.Zip, writerOptions, ct)
                .ConfigureAwait(false);
            await using (writer.ConfigureAwait(false))
            {
                while (await reader.MoveToNextEntryAsync(ct).ConfigureAwait(false))
                {
                    if (reader.Entry.Key is null)
                    {
                        continue;
                    }

                    if (!reader.Entry.IsDirectory)
                    {
                        var entryStream = await reader.OpenEntryStreamAsync(ct).ConfigureAwait(false);
                        await using (entryStream.ConfigureAwait(false))
                        {
                            await writer.WriteAsync(reader.Entry.Key, entryStream, reader.Entry.LastModifiedTime, ct)
                                .ConfigureAwait(false);
                        }
                        await Task.Yield();
                    }
                    else
                    {
                        await writer.WriteDirectoryAsync(reader.Entry.Key, reader.Entry.LastModifiedTime, ct)
                            .ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
