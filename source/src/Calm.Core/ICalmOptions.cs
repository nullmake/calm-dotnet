namespace Calm.Core;

/// <summary>
/// Provides configuration options for <see cref="CalmEngine"/>.
/// </summary>
public interface ICalmOptions
{
    /// <summary>
    /// Gets the maximum capacity of the message pump queue.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets an optional provider for time-based operations.
    /// </summary>
    TimeProvider TimeProvider { get; }

    /// <summary>
    /// Gets the threshold after which a task is considered stalled.
    /// </summary>
    TimeSpan WatchdogThreshold { get; }

    /// <summary>
    /// Gets a value indicating whether to enable logging of engine operations and diagnostics.
    /// </summary>
    bool EnableLogger { get; }
}
