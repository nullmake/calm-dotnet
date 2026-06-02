using System.Runtime.CompilerServices;

namespace Calm.Core.Engines;

/// <summary>
/// Represents the core execution engine that manages single-threaded,
/// safe execution within a dedicated context.
/// </summary>
public interface ICalmPump
{
    /// <summary>
    /// Gets the provider for UTC time.
    /// </summary>
    TimeProvider TimeProvider { get; }

    /// <summary>
    /// Gets a value indicating whether the current thread is the engine's execution thread.
    /// </summary>
    bool IsOnEngineThread { get; }

    /// <summary>
    /// Starts the engine pump.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Start(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Signals the engine to stop accepting new items and processes remaining items.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the shutdown signal completion.</returns>
    Task StopAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Signals the engine to stop accepting new items and processes remaining items.
    /// </summary>
    /// <param name="token">Optional cancellation token for waiting the shutdown.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the shutdown signal completion.</returns>
    Task StopAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Waits for the engine to shut down completely, respecting a grace period.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the final shutdown completion.</returns>
    Task WaitForShutdownAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Waits for the engine to shut down completely, respecting a grace period.
    /// </summary>
    /// <param name="token">Optional cancellation token for waiting the shutdown.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the final shutdown completion.</returns>
    Task WaitForShutdownAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Verifies that the current thread is the designated engine thread.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <exception cref="CalmAffinityException">Thrown if cross-thread access is detected.</exception>
    void VerifyContext(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Provides an awaiter that allows switching the execution context to the engine thread.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>An awaiter to switch to the engine thread.</returns>
    CalmSwitchAwaiter SwitchAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    #region ExecuteAsync
    /// <summary>
    /// Executes an asynchronous function on the engine thread and returns a task that completes when the operation finishes.
    /// The function receives a <see cref="CancellationToken"/> linked to the engine's shutdown and user cancellation.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Executes an asynchronous function on the engine thread and returns a task that completes when the operation finishes.
    /// The function receives a <see cref="CancellationToken"/> linked to the engine's shutdown and user cancellation.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
    #endregion

    #region ExecuteAsync<T>
    /// <summary>
    /// Executes an asynchronous function on the engine thread and returns a task that completes with the result.
    /// The function receives a <see cref="CancellationToken"/> linked to the engine's shutdown and user cancellation.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Executes an asynchronous function on the engine thread and returns a task that completes with the result.
    /// The function receives a <see cref="CancellationToken"/> linked to the engine's shutdown and user cancellation.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
    #endregion

    #region Schedule
    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
    #endregion

    #region Schedule with delay
    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>An awaitable object used to wait until the scheduled function begins execution.</returns>
    ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread after a specified delay.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="delay">The delay before execution.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that represents the asynchronous operation and contains the <see cref="ScheduleOperation"/>.</returns>
    Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
    #endregion
}
