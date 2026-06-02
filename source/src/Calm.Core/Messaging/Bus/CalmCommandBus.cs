using Calm.Core.Messaging.Handlers;
using Calm.Core.Messaging.Handlers.Registries;
using Microsoft.Extensions.Logging;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// A concrete implementation of the command bus that handles command registration and dispatch.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmCommandBus"/> class.
/// </remarks>
/// <param name="busCore">The underlying execution engine.</param>
/// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
internal sealed class CalmCommandBus(CalmBusCore busCore, CalmBusLog? logger) : ICalmCommandBus
{
    /// <summary>
    /// The underlying execution engine (CalmBus) used for posting message handling tasks.
    /// </summary>
    private readonly CalmBusCore _bus = busCore ?? throw new ArgumentNullException(nameof(busCore));

    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    private readonly CalmBusLog? _logger = logger;

    /// <summary>
    /// Registry for command handlers without response.
    /// </summary>
    private readonly SingleMessageHandlerRegistry _messageHandlers = new();

    /// <summary>
    /// Registry for command handlers with response.
    /// </summary>
    private readonly SingleRequestHandlerRegistry _requestHandlers = new();

    /// <summary>
    /// Registers a handler method using reflection.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The instance which has the method.</param>
    /// <exception cref="ArgumentNullException">The calmHandlerInfo parameter is null.</exception>
    internal void Register(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        ArgumentNullException.ThrowIfNull(calmHandlerInfo);

        if (calmHandlerInfo.ReturnValueType == typeof(void))
        {
            var messageHandler = MessageHandler.Create(calmHandlerInfo, instance);
            _messageHandlers.Register(messageHandler);
        }
        else
        {
            var requestHandler = RequestHandler.Create(calmHandlerInfo, instance);
            _requestHandlers.Register(requestHandler);
        }
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Register<TCommand>(Func<TCommand, CancellationToken, Task> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            var messageHandler = new MessageHandler<TCommand>(callback);

            _logger?.RegisteringHandler(LogLevel.Trace, messageHandler,
                memberName, filePath, lineNumber);

            _messageHandlers.Register(messageHandler);

            _logger?.RegisteredHandler(LogLevel.Information, messageHandler,
                memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Register<TCommand, TResponse>(
        Func<TCommand, CancellationToken, Task<TResponse>> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            var requestHandler = new RequestHandler<TCommand, TResponse>(callback);

            _logger?.RegisteringHandler(LogLevel.Trace, requestHandler,
                memberName, filePath, lineNumber);

            _requestHandlers.Register(requestHandler);

            _logger?.RegisteredHandler(LogLevel.Information, requestHandler,
                memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Unregister(
        string memberName, string filePath, int lineNumber)
    {
        try
        {
            _logger?.WriteLine(LogLevel.Trace, "[Clear] Removing all command handlers.",
                memberName, filePath, lineNumber);

            _messageHandlers.Clear();
            _requestHandlers.Clear();

            _logger?.WriteLine(LogLevel.Information, "[Clear] Removed all command handlers.",
                memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Unregisters a callback handler for a specific command type that does not produce a response.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The instance which has the method.</param>
    /// <exception cref="ArgumentNullException">The calmHandlerInfo parameter is null.</exception>
    internal void Unregister(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        ArgumentNullException.ThrowIfNull(calmHandlerInfo);

        var method = calmHandlerInfo.CreateMethod(instance);
        if (calmHandlerInfo.ReturnValueType == typeof(void))
        {
            _messageHandlers.Unregister(calmHandlerInfo.ParameterType, method);
        }
        else
        {
            _requestHandlers.Unregister(calmHandlerInfo.ParameterType, method);
        }
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Unregister<TCommand>(Func<TCommand, CancellationToken, Task> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            _logger?.UnregisteringHandler(LogLevel.Trace, callback, typeof(TCommand),
                memberName, filePath, lineNumber);

            if (_messageHandlers.Unregister(typeof(TCommand), callback))
            {
                _logger?.UnregisteredHandler(LogLevel.Information, callback, typeof(TCommand),
                    memberName, filePath, lineNumber);
            }
            else
            {
                _logger?.NoHandlersRegistered(LogLevel.Trace, typeof(TCommand));
            }
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Unregister<TCommand, TResponse>(
        Func<TCommand, CancellationToken, Task<TResponse>> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            _logger?.UnregisteringHandler(LogLevel.Trace, callback, typeof(TCommand),
                memberName, filePath, lineNumber);

            if (_requestHandlers.Unregister(typeof(TCommand), callback))
            {
                _logger?.UnregisteredHandler(LogLevel.Information, callback, typeof(TCommand),
                    memberName, filePath, lineNumber);
            }
            else
            {
                _logger?.NoHandlersRegistered(LogLevel.Trace, typeof(TCommand));
            }
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IReadOnlyMessageHandler> EnumerateMessageHandler()
        => _messageHandlers.AsEnumerable<IReadOnlyMessageHandler>();

    /// <inheritdoc/>
    public IEnumerable<IReadOnlyRequestHandler> EnumerateRequestHandler()
        => _requestHandlers.AsEnumerable<IReadOnlyRequestHandler>();

    /// <inheritdoc/>
    void ICalmCommandBus.Post<TCommand>(TCommand command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).Post(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    void ICalmCommandBus.Post<TCommand>(TCommand command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogPost(command, typeof(TCommand), "ICalmCommandBus.Post", memberName, filePath, lineNumber);

        _bus.Scheduler.Schedule(async ct =>
        {
            await ((ICalmCommandBus)this).SendAsync(command, ct, memberName, filePath, lineNumber)
                .ConfigureAwait(true);
        }, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    void ICalmCommandBus.Post<TResponse>(ICalmCommand<TResponse> command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).Post(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    void ICalmCommandBus.Post<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogPost(command, command.GetType(), "ICalmCommandBus.Post", memberName, filePath, lineNumber);

        _bus.Scheduler.Schedule(async ct =>
        {
            _ = await ((ICalmCommandBus)this).SendAsync(command, ct, memberName, filePath, lineNumber)
                .ConfigureAwait(true);
        }, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    Task ICalmCommandBus.PostAsync<TCommand>(TCommand command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).PostAsync(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task ICalmCommandBus.PostAsync<TCommand>(TCommand command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogPost(command, typeof(TCommand), "ICalmCommandBus.PostAsync", memberName, filePath, lineNumber);

        return _bus.Scheduler.ScheduleAsync(async ct =>
        {
            await ((ICalmCommandBus)this).SendAsync(command, ct, memberName, filePath, lineNumber)
                .ConfigureAwait(true);
        }, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    Task ICalmCommandBus.PostAsync<TResponse>(ICalmCommand<TResponse> command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).PostAsync(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task ICalmCommandBus.PostAsync<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogPost(command, command.GetType(), "ICalmCommandBus.PostAsync", memberName, filePath, lineNumber);

        return _bus.Scheduler.ScheduleAsync(async ct =>
        {
            await ((ICalmCommandBus)this).SendAsync(command, ct, memberName, filePath, lineNumber)
                .ConfigureAwait(true);
        }, memberName, filePath, lineNumber, token);
    }

    /// <summary>
    /// Logs the posting of an command.
    /// </summary>
    /// <param name="command">The command to be post.</param>
    /// <param name="commandType">The type of the command.</param>
    /// <param name="methodName">The name of the method performing the publication.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    private void LogPost(object command, Type commandType, string methodName,
        string memberName, string filePath, int lineNumber)
        => _bus.LogDispatchInfo("Scheduling a command handler to be executed.",
            command, commandType, methodName, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task ICalmCommandBus.SendAsync<TCommand>(TCommand command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).SendAsync(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task ICalmCommandBus.SendAsync<TCommand>(TCommand command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogSend(command, typeof(TCommand), "ICalmCommandBus.SendAsync", memberName, filePath, lineNumber);

        return _bus.SendAsync(_messageHandlers, command, memberName, filePath, lineNumber, token);
    }

    /// <inheritdoc/>
    Task<TResponse> ICalmCommandBus.SendAsync<TResponse>(ICalmCommand<TResponse> command,
        string memberName, string filePath, int lineNumber)
        => ((ICalmCommandBus)this).SendAsync(command, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task<TResponse> ICalmCommandBus.SendAsync<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(command);

        LogSend(command, command.GetType(), "ICalmCommandBus.SendAsync", memberName, filePath, lineNumber);

        return _bus.SendAsync(_requestHandlers, command, memberName, filePath, lineNumber, token);
    }

    /// <summary>
    /// Logs the sending of an command.
    /// </summary>
    /// <param name="command">The command to be send.</param>
    /// <param name="commandType">The type of the command.</param>
    /// <param name="methodName">The name of the method performing the publication.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    private void LogSend(object command, Type commandType, string methodName,
        string memberName, string filePath, int lineNumber)
        => _bus.LogDispatchInfo("Starting.", command, commandType, methodName,
            memberName, filePath, lineNumber);
}
