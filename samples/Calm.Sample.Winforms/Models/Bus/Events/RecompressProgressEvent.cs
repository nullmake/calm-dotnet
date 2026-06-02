using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Events;

/// <summary>
/// An event for recompressing the archive.
/// </summary>
/// <param name="FilePath">The archive file path.</param>
/// <param name="OriginalSize">The original size in bytes.</param>
/// <param name="NewSize">The new size in bytes.</param>
/// <param name="Status">The processing status.</param>
[CalmImmediate]
internal sealed record RecompressProgressEvent(
    string FilePath,
    long OriginalSize,
    long NewSize,
    string Status) : ICalmEvent;
