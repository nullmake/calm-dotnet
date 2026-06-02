using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Calm.Sample.Winforms.ViewModels.DataSources;

/// <summary>
/// The <see cref="ComboBox"/> and <see cref="ListControl"/> datasource item.
/// </summary>
/// <typeparam name="T">The type of the <see langword="Value"/></typeparam>
internal sealed partial class ListItemData<T> : ReactiveObject
{
    /// <summary>
    /// The name to display.
    /// </summary>
    [Reactive]
    private string _display = "";

    /// <summary>
    /// The name to display.
    /// </summary>
    [Reactive]
    private T _value = default!;
}
