using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Calm.Sample.Winforms.ViewModels.DataSources;

/// <summary>
/// Represents a row in the results grid.
/// </summary>
internal sealed partial class ProgressStatus : ReactiveObject
{
    /// <summary>
    /// Gets the file path.
    /// </summary>
    [Reactive]
    private string _filePath = string.Empty;

    /// <summary>
    /// Gets the original size.
    /// </summary>
    [Reactive]
    private long _originalSize;

    /// <summary>
    /// The backing field for the NewSize property.
    /// </summary>
    [Reactive]
    private long _newSize;

    /// <summary>
    /// The backing field for the Status property.
    /// </summary>
    [Reactive]
    private string _status = string.Empty;
}
