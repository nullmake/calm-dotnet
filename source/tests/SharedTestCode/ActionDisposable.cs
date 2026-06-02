namespace SharedTestCode;

/// <summary>
/// A class that executes the Action specified by `Dispose()`.
/// </summary>
/// <param name="action">The action to be executing by `Dispose()`.</param>
internal sealed class ActionDisposable(Action action) : IDisposable
{
    /// <inheritdoc/>
    public void Dispose()
    {
        action?.Invoke();
    }
}
