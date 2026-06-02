using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides an awaitable object that supports waiting for a scheduled delegate to begin execution.
/// </summary>
/// <param name="taskCompletionSource">The <see cref="TaskCompletionSource{TResult}"/>
/// that signals when the scheduled operation has started.</param>
[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks",
    Justification = "This struct is a dedicated wrapper for a standard TaskCompletionSource.Task.")]
public readonly struct CalmAwaitable(TaskCompletionSource<bool> taskCompletionSource)
{
    /// <summary>
    /// Performs an implicit upcast from <c>Task&lt;bool&gt;</c> to a non-generic <c>Task</c>.
    /// </summary>
    private readonly Task _task = taskCompletionSource.Task;

    /// <summary>
    /// Retrieves a <see cref="Task"/> object that represents this <see cref="CalmAwaitable"/>.
    /// </summary>
    /// <returns>The <see cref="Task"/> object.</returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods",
        Justification = "Methods for waiting as a Task object.")]
    public Task AsTask()
        => _task;

    /// <summary>
    /// Gets an awaiter used to await this <see cref="CalmAwaitable"/>.
    /// </summary>
    /// <returns>An awaiter instance.</returns>
    public readonly TaskAwaiter GetAwaiter()
        => _task.GetAwaiter();

    /// <summary>
    /// Configures an awaiter used to await this <see cref="CalmAwaitable"/>.
    /// </summary>
    /// <param name="continueOnCapturedContext">
    /// <see langword="true"/> to capture the current context and marshal the continuation back to it;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>An object used to await this operation.</returns>
    public readonly ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
        => _task.ConfigureAwait(continueOnCapturedContext);

    #region WaitAsync
    /// <summary>
    /// Asynchronously waits for the scheduled operation to start, supporting cancellation.
    /// </summary>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    public Task<bool> WaitAsync()
        => WaitAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <summary>
    /// Asynchronously waits for the scheduled operation to start, supporting cancellation.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    public Task<bool> WaitAsync(CancellationToken cancellationToken)
        => WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// Asynchronously waits for the scheduled operation to start, supporting cancellation.
    /// </summary>
    /// <param name="timeout">Timeout duration.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    public Task<bool> WaitAsync(TimeSpan timeout)
        => WaitAsync(timeout, CancellationToken.None);

    /// <summary>
    /// Asynchronously waits for the scheduled operation to start, supporting cancellation.
    /// </summary>
    /// <param name="timeout">Timeout duration.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (_task.IsCompleted)
        {
            return _task.Status is TaskStatus.RanToCompletion;
        }
        var delayTask = Task.Delay(timeout, cancellationToken);
        Task completedTask = await Task.WhenAny(_task, delayTask).ConfigureAwait(false);
        return completedTask != delayTask && _task.Status is TaskStatus.RanToCompletion;
    }
    #endregion
}
