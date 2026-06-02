#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a scheduled operation managed by the message loop,
/// providing distinct awaitable targets for both its start and completion lifecycles.
/// </summary>
public readonly struct ScheduleOperation
{
    /// <summary>
    /// Gets the <see cref="TaskCompletionSource{TResult}"/> that signals
    /// when the scheduled operation has started execution.
    /// </summary>
    internal TaskCompletionSource<bool> StartedTaskCompletionSource { get; }

    /// <summary>
    /// Gets the <see cref="TaskCompletionSource{TResult}"/> that signals
    /// when the scheduled operation has completed execution.
    /// </summary>
    internal TaskCompletionSource<bool> CompletedTaskCompletionSource { get; }

    /// <summary>
    /// Gets an awaitable object that supports waiting for the scheduled delegate to begin execution.
    /// </summary>
    public CalmAwaitable StartedAwaitable { get; }

    /// <summary>
    /// Gets an awaitable object that supports waiting for the scheduled delegate to complete execution.
    /// </summary>
    public CalmAwaitable CompletedAwaitable { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleOperation"/> struct.
    /// </summary>
    public ScheduleOperation()
    {
        StartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        StartedAwaitable = new(StartedTaskCompletionSource);
        CompletedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        CompletedAwaitable = new(CompletedTaskCompletionSource);
    }
}
