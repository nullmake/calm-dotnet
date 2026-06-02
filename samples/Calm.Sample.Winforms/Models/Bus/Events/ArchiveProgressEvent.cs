using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Events;

/// <summary>
/// An event for creating the archive.
/// </summary>
/// <param name="FilePath">The archive file path.</param>
/// <param name="Size">The current archive size in bytes.</param>
/// <param name="Status">The processing status.</param>
internal sealed record ArchiveProgressEvent(
    string FilePath,
    long Size,
    string Status) : ICalmEvent;
