using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.IO;
using Calm.Sample.Winforms.Models.Bus.Commands;
using Calm.Sample.Winforms.Models.Bus.Events;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms.Models.Services.Compression;

/// <summary>
/// Sample archive creator.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="calm">The calm engine instance.</param>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed class ArchiveCreatorService(ILogger<ArchiveCreatorService> logger, ICalm calm) : IDisposable
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
        _logger.LogInformation("Disposing '{Class}' instance.", nameof(ArchiveCreatorService));
        _calm.Unregister(this);
        _disposed = true;
    }
    #endregion

    /// <summary>
    /// Handles the <see cref="CreateSampleArchiveCommand"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="NotSupportedException">Not supported archive type.</exception>
    [CalmHandler]
    private async Task HandleCreateSampleArchiveCommandAsync(
        CreateSampleArchiveCommand command, CancellationToken token)
    {
        _logger.LogInformation("Handle comand: {Command}", command);

        var directory = Path.GetDirectoryName(command.Path);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        var extension = Path.GetExtension(command.Path);
        var size = extension.ToUpperInvariant() switch
        {
            ".ZIP" => await CreateSampleZipArchiveAsync(command.Path, command.OriginalSize, token),
            _ => throw new NotSupportedException($"\"{extension}\" is not supported."),
        };
        await _calm.Event.PublishAsync(new ArchiveProgressEvent(command.Path, size, "Created."), token);
    }

    /// <summary>
    /// Create a sample zip archive.
    /// </summary>
    /// <param name="path">The sample archive path.</param>
    /// <param name="size">File size before compression.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The created archive size.</returns>
    private static async Task<long> CreateSampleZipArchiveAsync(string path, long size, CancellationToken token)
    {
        var zip = File.OpenWrite(path);
        await using (zip.ConfigureAwait(false))
        {
            var writerOptions = new SharpCompress.Writers
                .WriterOptions(SharpCompress.Common.CompressionType.Deflate, 0);
            var writer = await SharpCompress.Writers.WriterFactory
                .OpenAsyncWriter(zip, SharpCompress.Common.ArchiveType.Zip, writerOptions, token)
                .ConfigureAwait(false);
            await using (writer.ConfigureAwait(false))
            {
                var rs = new RandomCharactorStream(size);
                await using (rs.ConfigureAwait(false))
                {
                    var modificationTime = TimeProvider.System.GetUtcNow().DateTime;
                    await writer
                        .WriteAsync("random-characters.txt", rs, modificationTime, token)
                        .ConfigureAwait(false);
                }
            }
            return zip.Length;
        }
    }
}
