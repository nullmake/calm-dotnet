using Calm.Core.Engines.Contexts;
using Calm.Core.Engines.SynchronizationContexts;
using Calm.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Calm.Core.Engines;

/// <summary>
/// The primary execution engine for CALM, providing a robust, single-threaded message pump
/// built on <see cref="System.Threading.Channels"/>. It ensures thread safety by construction
/// through dedicated execution contexts.
/// </summary>
internal sealed partial class CalmPump : ICalmPump, ICalmScheduler,
    ICalmSynchronizationContextDispatcher, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    private readonly CalmPumpLog? _logger;

    /// <summary>
    /// Observer for catching and reporting unhandled exceptions.
    /// </summary>
    private readonly ICalmErrorObserver? _errorObserver;

    /// <summary>
    /// The internal channel used to queue work items.
    /// </summary>
    private readonly Channel<CalmTask> _channel;

    /// <summary>
    /// Semaphore used to enforce logical capacity for external writers.
    /// </summary>
    private readonly SemaphoreSlim _capacitySemaphore;

    /// <summary>
    /// Source for tracking the lifecycle completion of the engine thread/loop.
    /// </summary>
    private readonly TaskCompletionSource<bool> _completionSource;

    /// <summary>
    /// Source for cancelling the internal message loop.
    /// </summary>
    private readonly CancellationTokenSource _loopCts;

    /// <summary>
    /// Source for notifying tasks to stop during the graceful shutdown sequence.
    /// </summary>
    private readonly CancellationTokenSource _shutdownCts;

    /// <summary>
    /// The number of active asynchronous operations (spanning multiple segments).
    /// </summary>
    private int _activeOperationsCount;

    /// <summary>
    /// Indicates whether the message loop is active.
    /// </summary>
    private bool _isLoopActive;

    /// <summary>
    /// Indicates whether the engine has been started.
    /// </summary>
    private int _isStarted;

    /// <summary>
    /// Indicates whether the engine is in the process of stopping.
    /// </summary>
    private volatile bool _isStopping;

    /// <summary>
    /// The dedicated thread for the engine loop.
    /// </summary>
    private Thread? _engineThread;

    /// <summary>
    /// The managed thread ID assigned to this engine's execution.
    /// </summary>
    private int _threadId;

    /// <summary>
    /// Metadata for the task currently being executed, used for stall reporting.
    /// </summary>
    private volatile CalmTaskInfo? _currentTaskInfo;

    /// <inheritdoc/>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// The threshold for detecting long-running tasks.
    /// </summary>
    private readonly TimeSpan _longRunningThreshold;

    /// <inheritdoc/>
    public bool IsOnEngineThread => Environment.CurrentManagedThreadId == Volatile.Read(ref _threadId);

    /// <inheritdoc/>
    bool ICalmScheduler.ScheduleRequired => !IsOnEngineThread;

    /// <summary>
    /// Gets a value indicating whether the engine is idle that is no tasks are currently executing.
    /// </summary>
    private bool IsIdle => !Volatile.Read(ref _isLoopActive)
        && Volatile.Read(ref _activeOperationsCount) is 0
        && _channel.Reader.Count is 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmPump"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the pump.</param>
    /// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
    public CalmPump(CalmOptions options, ILogger? logger = null)
    {
        _logger = logger is null ? null : new CalmPumpLog(logger, this);
        _logger?.Trace("Initializing CalmPump.");
        _errorObserver = options.ErrorObserver;
        TimeProvider = options.TimeProvider ?? TimeProvider.System;

        _channel = Channel.CreateBounded<CalmTask>(new BoundedChannelOptions(int.MaxValue)
        {
            SingleReader = true,
            SingleWriter = false,
        });
        _capacitySemaphore = new SemaphoreSlim(options.Capacity, options.Capacity);
        _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _loopCts = new();
        _shutdownCts = new();
        _activeOperationsCount = 0;
        _isLoopActive = false;
        _isStarted = 0;
        _isStopping = false;
        _engineThread = null;
        _threadId = -1;
        _currentTaskInfo = null;
        _longRunningThreshold = options.WatchdogThreshold;
    }

    #region IDisposable, IAsyncDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Lock for synchronizing the disposal process.
    /// </summary>
    private int _disposeLock;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed || Interlocked.CompareExchange(ref _disposeLock, 1, 0) is 1)
        {
            _logger?.AlreadyDisposed(LogLevel.Warning);
            return;
        }

        _logger?.Trace("[Dispose] Disposing CalmPump.");

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            // StopAsync is implemented to allow for synchronous waiting.
            StopAsync(cts.Token).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        _loopCts.Dispose();
        _shutdownCts.Dispose();
        _capacitySemaphore.Dispose();
        WaitUntilEngineThreadTerminated(TimeSpan.FromSeconds(5));

        _disposed = true;
        _logger?.Trace("[Dispose] CalmPump disposed.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed || Interlocked.CompareExchange(ref _disposeLock, 1, 0) is 1)
        {
            _logger?.AlreadyDisposed(LogLevel.Warning);
            return;
        }

        _logger?.Trace("[DisposeAsync] Disposing CalmPump.");

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            await StopAsync(cts.Token).ConfigureAwait(false);
        }

        _loopCts.Dispose();
        _shutdownCts.Dispose();
        _capacitySemaphore.Dispose();
        WaitUntilEngineThreadTerminated(TimeSpan.FromSeconds(5));

        _disposed = true;
        _logger?.Trace("[DisposeAsync] CalmPump disposed.");
    }

    /// <summary>
    /// Blocks the calling thread until the engine thread is terminated.
    /// </summary>
    /// <param name="timeout">The amount of time to wait for the thread to terminate.</param>
    private void WaitUntilEngineThreadTerminated(TimeSpan timeout)
    {
        var thread = _engineThread;
        if (thread is not null && Thread.CurrentThread != thread)
        {
            _logger?.Trace("Waiting for engine thread to exit.");
            thread.Join(timeout);
        }
    }
    #endregion

    #region Shutdown
    /// <inheritdoc/>
    public Task StopAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => StopAsync(CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            bool isOnThread = IsOnEngineThread;
            _isStopping = true;

            _logger?.Information("[StopAsync] Initiating shutdown sequence.",
                memberName, filePath, lineNumber);

            // Request cancellation for all currently running tasks.
            if (!_shutdownCts.IsCancellationRequested)
            {
                _logger?.ActiveCountAndReaderCount(LogLevel.Debug, "Requesting running tasks cancellation.");
#if NET8_0_OR_GREATER
                await _shutdownCts.CancelAsync().ConfigureAwait(false);
#else
                _shutdownCts.Cancel();
#endif
            }

            // If we're not on the engine thread, wait for all active tasks to respect the cancellation
            // and finish their current segment. This prevents abrupt termination of mid-execution logic.
            if (!isOnThread)
            {
                await WaitForShutdownCalmTaskAsync(token).ConfigureAwait(false);
            }

            // Close the channel to prevent any new tasks from being enqueued.
            _logger?.ActiveCountAndReaderCount(LogLevel.Debug,
                "Completing channel writer to stop accepting new tasks.");
            _channel.Writer.TryComplete();

            // Signal the main message loop to exit.
            if (!_loopCts.IsCancellationRequested)
            {
                _logger?.ActiveCountAndReaderCount(LogLevel.Debug, "Requesting Calm engine loop cancellation.");
#if NET8_0_OR_GREATER
                await _loopCts.CancelAsync().ConfigureAwait(false);
#else
                _loopCts.Cancel();
#endif
            }

            // Wait for the main message loop thread to exit gracefully.
            if (!isOnThread)
            {
                await WaitForShutdownAsync(token).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Waits for the calm tasks to shut down completely, respecting a grace period.
    /// </summary>
    /// <param name="token">Optional cancellation token for waiting the shutdown.</param>
    /// <returns>A task representing the final shutdown completion.</returns>
    private async Task WaitForShutdownCalmTaskAsync(CancellationToken token)
    {
        // Wait for all active asynchronous operations and Channel to complete.
        _logger?.ActiveCountAndReaderCount(LogLevel.Trace,
            "Waiting for active operations and channel to drain before stopping.");

        var count = 0;
        while (!IsIdle)
        {
            if (token.IsCancellationRequested)
            {
                _logger?.ActiveCountAndReaderCount(LogLevel.Warning,
                    "The wait for the active operation and channel draining has been cancelled.");
                break;
            }

            await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
            ++count;
            if (count % 10 is 0)
            {
                _logger?.ActiveCountAndReaderCount(LogLevel.Debug,
                    "Still waiting for active operations and channel to drain before stopping.");
            }
        }
    }

    /// <inheritdoc/>
    public Task WaitForShutdownAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => WaitForShutdownAsync(CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    [SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks",
        Justification = "To wait for the message loop to complete.")]
    public async Task WaitForShutdownAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.Trace("[WaitForShutdownAsync] Waiting for Calm engine loop to complete.",
                memberName, filePath, lineNumber);

            if (Interlocked.CompareExchange(ref _isStarted, 0, 0) is 0)
            {
                _completionSource.TrySetResult(true);
            }

            _logger?.ActiveCountAndReaderCount(LogLevel.Debug,
                "WaitForShutdownAsync awaiting completion source.");

            // Intentionally waiting for the message loop to complete via Task.WhenAny.
            var completedTask = await Task.WhenAny(_completionSource.Task, Task.Delay(Timeout.Infinite, token))
                .ConfigureAwait(false);
            if (completedTask == _completionSource.Task)
            {
                // Execute `await TaskCompletionSource.Task` to rethrow any exceptions that occur internally.
                await _completionSource.Task.ConfigureAwait(false);

                _logger?.ActiveCountAndReaderCount(LogLevel.Debug,
                    "WaitForShutdownAsync completed. Calm Engine loop has exited.");
            }
            else
            {
                _logger?.ActiveCountAndReaderCount(LogLevel.Warning,
                    "WaitForShutdownAsync canceled or timed out while waiting for Calm engine loop to complete.");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region Engine Loop
    /// <inheritdoc/>
    public void Start(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.Information("[Start] Starting Calm engine loop.",
                memberName, filePath, lineNumber);

            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) is not 0)
            {
                return;
            }

            using var ev = new ManualResetEventSlim(false);
            _engineThread = new Thread(() =>
            {
                Volatile.Write(ref _threadId, Environment.CurrentManagedThreadId);
                SynchronizationContext.SetSynchronizationContext(new CalmSynchronizationContext(this));
                ev.Set();
                RunLoop();
            })
            {
                IsBackground = true,
                Name = "Calm Thread",
            };
            _engineThread.Start();
            ev.Wait(_loopCts.Token);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Core synchronous message loop.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Errors are forwarded to the observer")]
    private void RunLoop()
    {
        _logger?.Information("Calm engine thread started.");
        try
        {
            while (!_loopCts.IsCancellationRequested)
            {
                if (!WaitNextTask(_loopCts.Token))
                {
                    break;
                }

                try
                {
                    Volatile.Write(ref _isLoopActive, true);

                    while (_channel.Reader.TryRead(out var calmTask))
                    {
                        CalmTelemetry.QueueDepth.Add(-1);

                        if (calmTask.IsCapacityReserved)
                        {
                            _capacitySemaphore.Release();
                        }

                        _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                            "Channel Read(Sync) completed. Invoking task.", calmTask.Metadata);

                        InvokeCalmTask(calmTask);
                    }
                }
                finally
                {
                    Volatile.Write(ref _isLoopActive, false);
                }
            }
        }
        catch (Exception ex)
        {
            // Catch any critical failure that would otherwise silently terminate the pump thread.
            var unwrapped = CalmExceptionHelper.Unwrap(ex);
            _logger?.Error(unwrapped, "Unhandled exception in Calm engine loop.");
            _errorObserver?.OnUnhandledException(unwrapped);
        }
        finally
        {
            Volatile.Write(ref _threadId, 0);
            _logger?.Information("Calm engine thread exiting.");
            _completionSource.TrySetResult(true);
        }
    }

    /// <summary>
    /// Waits the next task from the channel, updating engine state.
    /// </summary>
    /// <param name="token">Cancellation token to observe.</param>
    /// <returns>true: The next CalmTask is available, false: the channel is closed or loop canceled.</returns>
    private bool WaitNextTask(CancellationToken token)
    {
        try
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            // Block until a task is available.
            return _channel.Reader.WaitToReadAsync(token).AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
        catch (Exception ex) when (ex is OperationCanceledException or ChannelClosedException)
        {
            _logger?.Debug(ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Executes the given CalmTask segment. If the task is asynchronous, it starts the task
    /// and returns immediately after the first yield point.
    /// </summary>
    /// <param name="calmTask">CalmTask to be executed.</param>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Errors are forwarded to the observer")]
    private void InvokeCalmTask(CalmTask calmTask)
    {
        var startTime = TimeProvider.GetUtcNow();
        try
        {
            _currentTaskInfo = calmTask.Metadata;

            // Execute the operation segment. The handler is expected to return a Task
            // if it's asynchronous, or Task.CompletedTask/null if synchronous.
            var task = calmTask.OnExecuteAsync();

            // Handle the task completion. We need to ensure that any exceptions,
            // whether thrown synchronously or asynchronously, are captured and reported.
            if (task is not null)
            {
                if (task.IsFaulted)
                {
                    // Synchronous failure during task startup.
                    HandleTaskFailure(task.Exception);
                }
                else if (!task.IsCompleted)
                {
                    // The task is running asynchronously. Attach a continuation to handle
                    // any eventual failure and prevent UnobservedTaskException.
                    _ = task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            HandleTaskFailure(t.Exception);
                        }
                    }, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }
                else
                {
                    // do nothing.
                }
            }
        }
        catch (Exception ex)
        {
            // Catch exceptions thrown directly during the invocation of the delegate.
            HandleTaskFailure(ex);
        }
        finally
        {
            var endTime = TimeProvider.GetUtcNow();
            var duration = endTime - startTime;

            // Watchdog: Detect and report tasks that exceed the stall threshold.
            if (duration > _longRunningThreshold)
            {
                _logger?.Duration(LogLevel.Warning, "Long-running task detected.", duration);
                _errorObserver?.OnStall(new StallEventArgs(_currentTaskInfo, duration));
            }

            _currentTaskInfo = null;
        }
    }

    /// <summary>
    /// Handles task failures by unwrapping and notifying the observer.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    private void HandleTaskFailure(Exception exception)
    {
        var unwrapped = CalmExceptionHelper.Unwrap(exception);
        _logger?.Error(unwrapped, "Task segment failure.");
        _errorObserver?.OnUnhandledException(unwrapped);
    }
    #endregion

    #region Diagnostics
    /// <inheritdoc/>
    public void VerifyContext(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.Trace("[VerifyContext] Checking if on Calm engine thread.",
                memberName, filePath, lineNumber);
            var threadId = Volatile.Read(ref _threadId);
            if (threadId < 0)
            {
                return;
            }
            if (!IsOnEngineThread)
            {
                throw new CalmAffinityException(string.Format(CultureInfo.InvariantCulture,
                     "Cross-thread access detected. Expected Calm ThreadId: {0}, Actual ThreadId: {1}",
                     threadId, Environment.CurrentManagedThreadId));
            }
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region Awaiter
    /// <inheritdoc/>
    public CalmSwitchAwaiter SwitchAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.Trace("[SwitchAsync] Returning awaiter to switch to Calm engine thread.",
                memberName, filePath, lineNumber);
            return new(this);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region ICalmSynchronizationContextDispatcher
    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exception is set to TaskCompletionSource to propagate to the caller")]
    void ICalmSynchronizationContextDispatcher.Send(SendOrPostCallback d, object state)
    {
        if (IsOnEngineThread)
        {
            // When called from the engine thread, execute directly but wrap exceptions
            // to avoid being caught by InvokeCalmTask as an unhandled exception.
            // This ensures consistent behavior with the non-engine-thread path.
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                d(state);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            // Wait and rethrow to propagate the exception to the caller.
            tcs.Task.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            return;
        }

        _logger?.Trace("CalmSynchronizationContext.Send called from another thread."
            + "Dispatching to Calm engine thread.");

        SendAsync(async _ =>
        {
            d(state);
            return true;
        }, CancellationToken.None)
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
    }

    /// <inheritdoc/>
    void ICalmSynchronizationContextDispatcher.Post(SendOrPostCallback d, object state)
    {
        _logger?.Trace("CalmSynchronizationContext.Post called. Dispatching to Calm engine thread.");
        var metadata = new CalmTaskInfo(Guid.NewGuid(), "Continuation", string.Empty, 0, TimeProvider.GetUtcNow());
        Enqueue(new CalmTask(() =>
        {
            d(state);
            return Task.CompletedTask;
        }, metadata), forced: true);
    }
    #endregion

    #region ExecuteAsync
    /// <inheritdoc/>
    public Task ExecuteAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => ExecuteAsync(funcAsync, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task ExecuteAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.DispatchingToEngineThread(LogLevel.Trace, "ExecuteAsync",
                memberName, filePath, lineNumber);

            ThrowIfStopping(memberName, filePath, lineNumber);

            async Task<bool> FuncWithBooleanResponseAsync(CancellationToken ct)
            {
                await funcAsync(ct).ConfigureAwait(true);
                return true;
            }
            return SendAsync(FuncWithBooleanResponseAsync, token, memberName, filePath, lineNumber);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region ExecuteAsync<T>
    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => ExecuteAsync(funcAsync, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.DispatchingToEngineThread(LogLevel.Trace, "ExecuteAsync", typeof(T),
                memberName, filePath, lineNumber);

            ThrowIfStopping(memberName, filePath, lineNumber);

            return SendAsync(funcAsync, token, memberName, filePath, lineNumber);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region Schedule
    #region Sync
    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Schedule(funcAsync, default(CancellationToken), memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        _logger?.DispatchingToEngineThread(LogLevel.Trace, "Schedule",
            memberName, filePath, lineNumber);
        return ((ICalmScheduler)this).Schedule(funcAsync, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    ScheduleOperation ICalmScheduler.Schedule(Func<CancellationToken, Task> funcAsync,
         string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        var operation = new ScheduleOperation();
        Schedule(funcAsync, operation, memberName, filePath, lineNumber, token);
        return operation;
    }

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="operation">The scheduled operation.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <exception cref="CalmEngineStoppingException">The engine is stopped and cannot accept new tasks.</exception>
    private void Schedule(Func<CancellationToken, Task> funcAsync, ScheduleOperation operation,
         string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        try
        {
            ThrowIfStopping(memberName, filePath, lineNumber);

            var calmTask = CreateScheduleTask(funcAsync, operation, memberName, filePath, lineNumber, token);
            Enqueue(calmTask);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    #region Async
    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => ScheduleAsync(funcAsync, default(CancellationToken), memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        _logger?.DispatchingToEngineThread(LogLevel.Trace, "ScheduleAsync",
            memberName, filePath, lineNumber);
        return ((ICalmScheduler)this).ScheduleAsync(funcAsync, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    async Task<ScheduleOperation> ICalmScheduler.ScheduleAsync(Func<CancellationToken, Task> funcAsync,
         string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        var operation = new ScheduleOperation();
        await ScheduleAsync(funcAsync, operation, memberName, filePath, lineNumber, token).ConfigureAwait(false);
        return operation;
    }

    /// <summary>
    /// Schedules an asynchronous function to be executed on the engine thread without waiting for its completion.
    /// This is the primary fire-and-forget mechanism.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="operation">The scheduled operation.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="CalmEngineStoppingException">The engine is stopped and cannot accept new tasks.</exception>
    private async Task ScheduleAsync(Func<CancellationToken, Task> funcAsync, ScheduleOperation operation,
         string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        try
        {
            ThrowIfStopping(memberName, filePath, lineNumber);

            var calmTask = CreateScheduleTask(funcAsync, operation, memberName, filePath, lineNumber, token);
            await EnqueueAsync(calmTask, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
    #endregion

    /// <summary>
    /// Creates a <see cref="CalmTask"/> for a scheduled operation.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <param name="operation">The scheduled operation.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">User-provided cancellation token.</param>
    /// <returns>A <see cref="CalmTask"/> that encapsulates the scheduled operation.</returns>
    private CalmTask CreateScheduleTask(Func<CancellationToken, Task> funcAsync, ScheduleOperation operation,
         string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        var metadata = new CalmTaskInfo(Guid.NewGuid(), memberName, filePath, lineNumber,
            TimeProvider.GetUtcNow());
        async Task? OnExecuteAsync()
        {
            Interlocked.Increment(ref _activeOperationsCount);

            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                "Scheduling task for execution.", metadata);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, token);
            Exception? thrownException = null;
            try
            {
                // Ensure that scheduled tasks do not inherit the Unit of Work state
                // from their creator's ExecutionContext.
                CalmContext.SetCurrentState(null);

                operation.StartedTaskCompletionSource.SetResult(true);
                await funcAsync(cts.Token).ConfigureAwait(true);
                operation.CompletedTaskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                thrownException = ex;
                throw;
            }
            finally
            {
                // If the task threw an exception, propagate it to the CompletedTaskCompletionSource
                // so that any awaiter does not hang indefinitely.
                if (thrownException is not null
                    && !operation.CompletedTaskCompletionSource.Task.IsCompleted)
                {
                    operation.CompletedTaskCompletionSource.SetException(thrownException);
                }

                Interlocked.Decrement(ref _activeOperationsCount);
                _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                    "Finished executing scheduled task.", metadata);
            }
        }
        return new CalmTask(OnExecuteAsync, metadata);
    }
    #endregion

    #region Schedule with delay
    #region Sync
    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Schedule(funcAsync, delay, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        _logger?.DispatchingToEngineThread(LogLevel.Trace, "Schedule", delay,
            memberName, filePath, lineNumber);
        return ((ICalmScheduler)this).Schedule(funcAsync, delay, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    ScheduleOperation ICalmScheduler.Schedule(
        Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        ThrowIfStopping(memberName, filePath, lineNumber);

        var operation = new ScheduleOperation();
        if (delay <= TimeSpan.Zero)
        {
            _logger?.Warning("Schedule with delay called with non-positive delay. Executing immediately.");
            Schedule(funcAsync, operation, memberName, filePath, lineNumber, token);
        }
        else
        {
            _ = TimeProvider.CreateTimer(
                _ => Schedule(funcAsync, operation, memberName, filePath, lineNumber, token),
                state: null,
                dueTime: delay,
                period: Timeout.InfiniteTimeSpan);
        }
        return operation;
    }
    #endregion

    #region Async
    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => ScheduleAsync(funcAsync, delay, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        _logger?.DispatchingToEngineThread(LogLevel.Trace, "ScheduleAsync", delay,
            memberName, filePath, lineNumber);
        return ((ICalmScheduler)this).ScheduleAsync(funcAsync, delay, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    async Task<ScheduleOperation> ICalmScheduler.ScheduleAsync(
        Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        ThrowIfStopping(memberName, filePath, lineNumber);

        var operation = new ScheduleOperation();
        if (delay <= TimeSpan.Zero)
        {
            _logger?.Warning("ScheduleAsync with delay called with non-positive delay. Executing immediately.");
            await ScheduleAsync(funcAsync, operation, memberName, filePath, lineNumber, token).ConfigureAwait(false);
        }
        else
        {
            _ = TimeProvider.CreateTimer(
                _ => Schedule(funcAsync, operation, memberName, filePath, lineNumber, token),
                state: null,
                dueTime: delay,
                period: Timeout.InfiniteTimeSpan);
        }
        return operation;
    }
    #endregion
    #endregion

    #region Send
    /// <summary>
    /// Executes the specified asynchronous operation and returns a task representing its completion,
    /// capturing exceptions and cancellation.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the asynchronous operation.</typeparam>
    /// <param name="funcAsync">The operation to execute, which receives a TaskCompletionSource used to
    /// signal completion or failure, and a CancellationToken for cancellation support. </param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the operation signals completion, cancellation, or failure.
    /// The task contains the result of type T, or is faulted or canceled as appropriate.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exception is set to TaskCompletionSource to propagate to the caller")]
    private async Task<T> SendAsync<T>(
        Func<CancellationToken, Task<T>> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var metadata = new CalmTaskInfo(Guid.NewGuid(), memberName, filePath, lineNumber, TimeProvider.GetUtcNow());

        async Task? OnExecuteAsync()
        {
            Interlocked.Increment(ref _activeOperationsCount);

            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                "Starting SendAsync task execution.", metadata);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, token);
            try
            {
                // Ensure that independent tasks (ExecuteAsync) do not inherit the
                // Unit of Work state from their creator's ExecutionContext.
                CalmContext.SetCurrentState(null);

                var result = await funcAsync(cts.Token).ConfigureAwait(true);
                tcs.SetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                // Set the exception to the TaskCompletionSource to propagate it to the caller.
                // Do not rethrow - the exception is handled by the caller via await.
                tcs.SetException(ex);
            }
            finally
            {
                Interlocked.Decrement(ref _activeOperationsCount);
                _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                    "Finished SendAsync task execution.", metadata);
            }
        }

        await EnqueueAsync(new CalmTask(OnExecuteAsync, metadata), token).ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }
    #endregion

    #region Enqueue
    /// <summary>
    /// Enqueues a task for execution on the engine thread, handling telemetry and context flow.
    /// </summary>
    /// <param name="calmTask">The task to enqueue for execution.</param>
    /// <param name="forced">True to add the item even if the queue has reached its capacity.</param>
    private void Enqueue(CalmTask calmTask, bool forced = false)
    {
        bool isCapacityReserved = _capacitySemaphore.Wait(0, CancellationToken.None);
        if (isCapacityReserved)
        {
            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                "Enqueuing task for execution.", calmTask.Metadata);
        }
        else
        {
            if (forced || IsOnEngineThread)
            {
                _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Warning,
                    "Enqueuing task because the engine thread cannot be blocked.", calmTask.Metadata);
                isCapacityReserved = false;
            }
            else
            {
                try
                {
                    // External threads wait until space is available in the logical queue.
                    _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                        "Waiting for available capacity to enqueue task.", calmTask.Metadata);

                    _capacitySemaphore.Wait(_shutdownCts.Token);
                    isCapacityReserved = true;
                }
                catch (OperationCanceledException ex)
                {
                    _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Warning, ex,
                        "Failed to enqueue task due to cancellation. (Async)", calmTask.Metadata);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Error, ex,
                        "Failed to enqueue task. (Async)", calmTask.Metadata);
                    throw;
                }
            }
        }
        WriteChannel(calmTask, isCapacityReserved);
    }

    /// <summary>
    /// Enqueues a task for execution on the engine thread, handling telemetry and context flow.
    /// </summary>
    /// <param name="calmTask">The task to enqueue for execution.</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnqueueAsync(CalmTask calmTask, CancellationToken token)
    {
        try
        {
            // External threads wait until space is available in the logical queue.
            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Trace,
                "Waiting for available capacity to enqueue task.", calmTask.Metadata);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_shutdownCts.Token, token);
            await _capacitySemaphore.WaitAsync(cts.Token).ConfigureAwait(false);
            WriteChannel(calmTask, true);
        }
        catch (OperationCanceledException ex)
        {
            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Warning, ex,
                "Failed to enqueue task due to cancellation. (Async)", calmTask.Metadata);
            throw;
        }
        catch (Exception ex)
        {
            _logger?.ActiveCountAndReaderCountAndTaskInfo(LogLevel.Error, ex,
                "Failed to enqueue task. (Async)", calmTask.Metadata);
            throw;
        }
    }

    /// <summary>
    /// Enqueues a task into the internal channel and manages queue depth telemetry.
    /// </summary>
    /// <param name="calmTask">The task to enqueue for execution.</param>
    /// <param name="isCapacityReserved">Indicates whether a slot in the logical queue was reserved
    /// for this task.</param>
    /// <exception cref="CalmEngineStoppingException">
    /// Thrown when the channel is closed because the engine is stopping and the task could not be enqueued.
    /// </exception>
    private void WriteChannel(CalmTask calmTask, bool isCapacityReserved)
    {
        // Decorate the task with the current execution and activity context.
        var decoratedTask = DecorateCalmTask(calmTask);
        decoratedTask.IsCapacityReserved = isCapacityReserved;

        if (_channel.Writer.TryWrite(decoratedTask))
        {
            CalmTelemetry.QueueDepth.Add(1);
        }
        else
        {
            // The channel is closed (engine is stopping). Release the reserved capacity
            // slot if applicable, and notify the caller that the task was not enqueued.
            if (isCapacityReserved)
            {
                _capacitySemaphore.Release();
            }
            throw new CalmEngineStoppingException();
        }
    }

    /// <summary>
    /// Decorates the given CalmTask's handler with additional logic to manage SynchronizationContext,
    /// ExecutionContext flow, and Telemetry tracing.
    /// </summary>
    /// <param name="calmTask">Original CalmTask</param>
    /// <returns>Decorated CalmTask.</returns>
    [SuppressMessage("Usage", "MA0100:Await task before disposing of resources",
            Justification = "The Activity represents only the synchronous processing segment on the engine thread. "
                + "To accurately measure message pump occupancy, the Activity must be disposed of "
                + "once the synchronous execution segment completes, even if the task itself is asynchronous.")]
    private CalmTask DecorateCalmTask(CalmTask calmTask)
    {
        // Capture the current ExecutionContext and Activity to flow them to the engine thread.
        var context = ExecutionContext.Capture();
        var parentContext = Activity.Current?.Context ?? default;

        Task? WrappedOnExecuteAsync()
        {
            // Start a consumer activity for telemetry.
            using var activity = CalmTelemetry.ActivitySource.StartActivity(
                "CalmPump.Process", ActivityKind.Consumer, parentContext);

            CalmTelemetry.EnrichActivity(activity, calmTask.Metadata);

            var sw = Stopwatch.StartNew();
            try
            {
                // Run the task within the captured ExecutionContext if available.
                if (context is not null)
                {
                    Task? task = null;
                    ExecutionContext.Run(context, _ =>
                    {
                        task = ExecuteWithContextAsync(calmTask);
                    }, null);
                    return task;
                }
                return ExecuteWithContextAsync(calmTask);
            }
            finally
            {
                // Record processing metrics.
                sw.Stop();
                CalmTelemetry.ProcessingDuration.Record(sw.Elapsed.TotalMilliseconds);
                CalmTelemetry.MessagesProcessed.Add(1);
            }
        }

        return calmTask with
        {
            OnExecuteAsync = WrappedOnExecuteAsync,
        };
    }

    /// <summary>
    /// Executes the core operation of a CalmTask within the correct SynchronizationContext and task context.
    /// </summary>
    /// <param name="calmTask">The task to execute.</param>
    /// <returns>A task representing the completion of the execution segment.</returns>
    private Task? ExecuteWithContextAsync(CalmTask calmTask)
    {
        var synchronizationContext = SynchronizationContext.Current;
        if (synchronizationContext is not CalmSynchronizationContext)
        {
            if (synchronizationContext is null)
            {
                // ExecutionContext.Run may reset SynchronizationContext to null on some platforms
                // (e.g., .NET Framework). This is expected behavior.
                _logger?.Trace("SynchronizationContext was reset to null by ExecutionContext.Run. "
                    + "Resetting to CalmSynchronizationContext.");
            }
            else
            {
                // SynchronizationContext is set to a non-null value other than
                // CalmSynchronizationContext, it may indicate user code or a third-party library
                // has modified it, which could lead to unexpected behavior.
                _errorObserver?.OnContextLeaked();
                _logger?.Warning("SynchronizationContext was changed unexpectedly to "
                    + $"{synchronizationContext.GetType().FullName}. "
                    + "Resetting to CalmSynchronizationContext.");
            }
            SynchronizationContext.SetSynchronizationContext(new CalmSynchronizationContext(this));
        }

        try
        {
            CalmContext.SetCurrentTask(calmTask.Metadata);
            return calmTask.OnExecuteAsync();
        }
        finally
        {
            CalmContext.SetCurrentTask(null);
        }
    }
    #endregion

    /// <summary>
    /// Throws a <see cref="CalmEngineStoppingException"/> if the engine is stopping
    /// and the call is not from the engine thread.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <exception cref="CalmEngineStoppingException">If the engine is stopping
    /// and the call is not from the engine thread.</exception>
    private void ThrowIfStopping(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        if (_isStopping && !IsOnEngineThread)
        {
            var ex = new CalmEngineStoppingException();
            _logger?.Error(ex, ex.Message, memberName, filePath, lineNumber);
            throw ex;
        }
    }
}
