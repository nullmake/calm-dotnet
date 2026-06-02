using Calm.Core.Messaging.Handlers;
using Calm.Core.Messaging.Handlers.Registries;
using Microsoft.Extensions.Logging;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// A concrete implementation of the event bus that handles event registration and dispatch.
/// </summary>
/// <param name="busCore">The underlying execution engine.</param>
/// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
internal sealed class CalmEventBus(CalmBusCore busCore, CalmBusLog? logger) : ICalmEventBus
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
    /// Registry for event handlers.
    /// </summary>
    private readonly MultipleMessageHandlerRegistry _messageHandlers = new();

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

        var messageHandler = MessageHandler.Create(calmHandlerInfo, instance);
        _messageHandlers.Register(messageHandler);
    }

    /// <inheritdoc/>
    void ICalmEventBus.Register<TEvent>(Func<TEvent, CancellationToken, Task> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            var messageHandler = new MessageHandler<TEvent>(callback);

            _logger?.RegisteringHandler(LogLevel.Trace, messageHandler, memberName, filePath, lineNumber);

            _messageHandlers.Register(messageHandler);

            _logger?.RegisteredHandler(LogLevel.Information, messageHandler, memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    void ICalmEventBus.Unregister(
        string memberName, string filePath, int lineNumber)
    {
        try
        {
            _logger?.WriteLine(LogLevel.Trace, "[Clear] Removing all event handlers.",
                memberName, filePath, lineNumber);

            _messageHandlers.Clear();

            _logger?.WriteLine(LogLevel.Information, "[Clear] Removed all event handlers.",
                memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Unregisters a callback handler for a specific event type.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The instance which has the method.</param>
    /// <exception cref="ArgumentNullException">The calmHandlerInfo parameter is null.</exception>
    internal void Unregister(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        ArgumentNullException.ThrowIfNull(calmHandlerInfo);

        var messageType = calmHandlerInfo.ParameterType;
        var method = calmHandlerInfo.CreateMethod(instance);
        _messageHandlers.Unregister(messageType, method);
    }

    /// <inheritdoc/>
    void ICalmEventBus.Unregister<TEvent>(Func<TEvent, CancellationToken, Task> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            _logger?.UnregisteringHandler(LogLevel.Trace, callback, typeof(TEvent),
                memberName, filePath, lineNumber);
            if (_messageHandlers.Unregister(typeof(TEvent), callback))
            {
                _logger?.UnregisteredHandler(LogLevel.Information, callback, typeof(TEvent),
                    memberName, filePath, lineNumber);
            }
            else
            {
                _logger?.NoHandlersRegistered(LogLevel.Trace, typeof(TEvent),
                    memberName, filePath, lineNumber);
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
    void ICalmEventBus.Publish<TEvent>(TEvent @event,
        string memberName, string filePath, int lineNumber)
        => ((ICalmEventBus)this).Publish(@event, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    void ICalmEventBus.Publish<TEvent>(TEvent @event, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var scheduled = _bus.Publish(_messageHandlers, @event, memberName, filePath, lineNumber, token);
        LogPublish(@event, typeof(TEvent), scheduled, "ICalmEventBus.Publish", memberName, filePath, lineNumber);
    }

    /// <inheritdoc/>
    Task ICalmEventBus.PublishAsync<TEvent>(TEvent @event,
        string memberName, string filePath, int lineNumber)
        => ((ICalmEventBus)this).PublishAsync(@event, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    async Task ICalmEventBus.PublishAsync<TEvent>(TEvent @event, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var scheduled = await _bus.PublishAsync(_messageHandlers, @event, memberName, filePath, lineNumber, token)
            .ConfigureAwait(false);
        LogPublish(@event, typeof(TEvent), scheduled, "ICalmEventBus.PublishAsync", memberName, filePath, lineNumber);
    }

    /// <summary>
    /// Logs the publication of an event.
    /// </summary>
    /// <param name="event">The event being published.</param>
    /// <param name="eventType">The type of the <paramref name="event"/></param>
    /// <param name="scheduled">true if the event was scheduled; false if it was deferred.</param>
    /// <param name="methodName">The name of the method performing the publication.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    private void LogPublish(object @event, Type eventType, bool scheduled, string methodName,
        string memberName, string filePath, int lineNumber)
    {
        var message = scheduled
            ? "Scheduling an message handler to be executed."
            : "Defer publication to the outbox.";

        _bus.LogDispatchInfo(message, @event, eventType, methodName,
            memberName, filePath, lineNumber);
    }
}
