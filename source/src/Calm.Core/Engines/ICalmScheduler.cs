namespace Calm.Core.Engines;

/// <summary>
/// Represents an scheduler for calm engine.
/// </summary>
internal interface ICalmScheduler
{
    /// <summary>
    /// Whether you need to use the Schedule method to execute asynchronous functions.
    /// </summary>
    bool ScheduleRequired { get; }

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync,
        string memberName, string filePath, int lineNumber, CancellationToken token = default);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        string memberName, string filePath, int lineNumber, CancellationToken token = default);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync,
        string memberName, string filePath, int lineNumber, CancellationToken token = default);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        string memberName, string filePath, int lineNumber, CancellationToken token = default);
}
