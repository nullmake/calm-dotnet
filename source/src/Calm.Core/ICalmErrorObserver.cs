namespace Calm.Core;

/// <summary>
/// Defines an observer interface for capturing and handling engine-level events
/// such as unhandled exceptions and stalls.
/// </summary>
public interface ICalmErrorObserver
{
    /// <summary>
    /// Invoked when an unhandled exception is caught during the engine's message loop.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    void OnUnhandledException(Exception exception);

    /// <summary>
    /// Invoked when an engine stall is detected.
    /// </summary>
    /// <param name="e">Event arguments containing information about the stall.</param>
    void OnStall(StallEventArgs e);

    /// <summary>
    /// Called when the CalmSynchronizationContext is lost after a task execution,
    /// likely due to an improper use of .ConfigureAwait(false).
    /// </summary>
    void OnContextLeaked();
}
