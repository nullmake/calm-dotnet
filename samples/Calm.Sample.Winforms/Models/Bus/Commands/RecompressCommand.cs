using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Commands;

/// <summary>
/// Command to scan a folder for archives.
/// </summary>
/// <param name="FolderPath">The folder path to scan.</param>
/// <param name="Recursive">True to scan subfolders.</param>
internal sealed record RecompressCommand(string FolderPath, bool Recursive) : ICalmCommand
{
    /// <summary>
    /// The wait time immediately before reading the stream.
    /// </summary>
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;
}
