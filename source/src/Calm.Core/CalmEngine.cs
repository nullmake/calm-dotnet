using Calm.Core.Diagnostics;
using Calm.Core.Engines;
using Calm.Core.Messaging;
using Calm.Core.Messaging.Bus;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Calm.Core;

/// <summary>
/// The standard entry point for CALM, providing a unified facade for execution and messaging.
/// </summary>
public sealed class CalmEngine : ICalm, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    private readonly CalmLog? _logger;

    /// <summary>
    /// The underlying message pump responsible for scheduling and executing tasks.
    /// </summary>
    private readonly CalmPump _pump;

    /// <summary>
    /// Provides the CALM messaging system.
    /// </summary>
    private readonly CalmBus _bus;

    /// <inheritdoc/>
    public ICalmOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngine"/> class.
    /// </summary>
    public CalmEngine()
        : this(new(), null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngine"/> class.
    /// </summary>
    /// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
    public CalmEngine(ILogger? logger)
        : this(new(), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngine"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the engine.</param>
    public CalmEngine(CalmOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngine"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the engine.</param>
    /// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
    public CalmEngine(CalmOptions options, ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(options);

        Options = options;

        // Initialize diagnostics and logging.
        _logger = logger is null ? null : new CalmLog(logger);
        _logger?.AssemblyInfo(LogLevel.Information);
        _logger?.EngineOptions(LogLevel.Information, options);
        _logger?.LoggerInfo(LogLevel.Information);

        // Initialize core components: the message pump and the message bus.
        _pump = new(options, logger);
        _bus = new(_pump, options, logger);
    }

    #region IDisposable, IAsyncDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            _logger?.AlreadyDisposed(LogLevel.Warning);
            return;
        }

        // Gracefully shut down the message pump during disposal.
        _logger?.WriteLine(LogLevel.Information, "[Dispose] Disposing CalmEngine.");
        _pump.Dispose();

        _disposed = true;
        _logger?.WriteLine(LogLevel.Information, "[Dispose] CalmEngine disposed.");

    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            _logger?.AlreadyDisposed(LogLevel.Warning);
            return;
        }

        // Gracefully shut down the message pump during asynchronous disposal.
        _logger?.WriteLine(LogLevel.Information, "[DisposeAsync] Disposing CalmEngine.");
        await _pump.DisposeAsync().ConfigureAwait(false);

        _disposed = true;
        _logger?.WriteLine(LogLevel.Information, "[DisposeAsync] CalmEngine disposed.");
    }
    #endregion

    #region ICalmPump delegation
    //
    // The following members delegate execution and scheduling responsibilities to the internal CalmPump.
    //

    /// <inheritdoc/>
    public TimeProvider TimeProvider => _pump.TimeProvider;

    /// <inheritdoc/>
    public bool IsOnEngineThread => _pump.IsOnEngineThread;

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ExecuteAsync(funcAsync, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ExecuteAsync(funcAsync, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task ExecuteAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ExecuteAsync(funcAsync, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task ExecuteAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ExecuteAsync(funcAsync, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.Schedule(funcAsync, default(CancellationToken), memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.Schedule(funcAsync, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.Schedule(funcAsync, delay, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public ScheduleOperation Schedule(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.Schedule(funcAsync, delay, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ScheduleAsync(funcAsync, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ScheduleAsync(funcAsync, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ScheduleAsync(funcAsync, delay, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task<ScheduleOperation> ScheduleAsync(Func<CancellationToken, Task> funcAsync, TimeSpan delay, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.ScheduleAsync(funcAsync, delay, token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public CalmSwitchAwaiter SwitchAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.SwitchAsync(memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Start(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.Start(memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task StopAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.StopAsync(CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.StopAsync(token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task WaitForShutdownAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.WaitForShutdownAsync(CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public Task WaitForShutdownAsync(CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.WaitForShutdownAsync(token, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void VerifyContext(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _pump.VerifyContext(memberName, filePath, lineNumber);
    #endregion

    #region ICalm Bus Properties
    //
    // The following members delegate messaging, handler registration, and discovery to the internal CalmBus.
    //

    /// <inheritdoc/>
    public ICalmCommandBus Command => _bus.Command;

    /// <inheritdoc/>
    public ICalmQueryBus Query => _bus.Query;

    /// <inheritdoc/>
    public ICalmEventBus Event => _bus.Event;

    /// <inheritdoc/>
    public void Register(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Register(instance, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(object instance, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Register(instance, registrationFilter, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Register(type, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(Type type, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Register(type, registrationFilter, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Unregister(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Unregister(memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Unregister(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Unregister(instance, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Unregister(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => _bus.Unregister(type, memberName, filePath, lineNumber);
    #endregion
}
