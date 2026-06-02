using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Commands;

/// <summary>
/// Command to create a sample archive.
/// </summary>
/// <param name="Path">The archive path.</param>
/// <param name="OriginalSize">Data size before archiving.</param>
internal sealed record CreateSampleArchiveCommand(string Path, long OriginalSize) : ICalmCommand
{
}
