using Calm.Core.Engines;
using Calm.Core.Engines.Contexts;
using Calm.Core.Exceptions;
using Calm.Core.Messaging.Handlers.Registries;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// Provides the core execution engine for the CALM messaging system.
/// This class manages the message pump, error observer, and unit of work execution.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmBusCore"/> class.
/// </remarks>
/// <param name="scheduler">The scheduler for calm engine.</param>
/// <param name="options">The configuration options for the pump.</param>
/// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
internal sealed class CalmBusCore(ICalmScheduler scheduler, CalmOptions options, CalmBusLog? logger)
{
    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    private readonly CalmBusLog? _logger = logger;

    /// <summary>
    /// Gets the error observer for receiving error notifications.
    /// </summary>
    public ICalmErrorObserver? ErrorObserver { get; } = options.ErrorObserver;

    /// <summary>
    /// Gets the underlying message pump.
    /// </summary>
    public ICalmScheduler Scheduler { get; } = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

    /// <summary>
    /// Sends a command asynchronously.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="registry">The message handler registry.</param>
    /// <param name="message">The message to be handled.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are propagated via TaskCompletionSource")]
    public async Task SendAsync<TMessage>(
        SingleMessageHandlerRegistry registry, TMessage message,
        string memberName, string filePath, int lineNumber, CancellationToken token)
        where TMessage : ICalmMessage
    {
        var messageType = typeof(TMessage);
        var messageHandler = registry.GetHandler(messageType);
        var metadata = CalmMessageMetadata.Get(messageType);

        var currentState = CalmContext.CurrentState;
        if (currentState is not null)
        {
            // Nested: Direct execution on engine thread
            try
            {
                if (!metadata.SuppressLog)
                {
                    _logger?.DispatchInfo(LogLevel.Trace,
                        "Executing nested message handler.", "SendAsync", message);
                }
                await messageHandler.HandleAsync(message, token).ConfigureAwait(true);
                return;
            }
            catch (Exception ex)
            {
                var unwrapped = CalmExceptionHelper.Unwrap(ex);
                _logger?.Error(unwrapped, "Unhandled exception while executing message handler.");
                ErrorObserver?.OnUnhandledException(unwrapped);
                throw;
            }
        }

        // Root call
        if (!Scheduler.ScheduleRequired)
        {
            if (!metadata.SuppressLog)
            {
                _logger?.DispatchInfo(LogLevel.Trace, "Executing message handler.", "SendAsync", message);
            }
            await ExecuteRootUoWAsync(() => messageHandler.HandleAsync(message, token)).ConfigureAwait(true);
            return;
        }

        // Outside: Schedule and Wait
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await Scheduler.ScheduleAsync(async passedToken =>
        {
            try
            {
                if (!metadata.SuppressLog)
                {
                    _logger?.DispatchInfo(LogLevel.Trace,
                        "Executing message handler on engine thread.", "SendAsync", message);
                }
                await ExecuteRootUoWAsync(() => messageHandler.HandleAsync(message, passedToken))
                    .ConfigureAwait(true);
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(CalmExceptionHelper.Unwrap(ex));
            }
        }, memberName, filePath, lineNumber, token).ConfigureAwait(false);
        await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a query asynchronously and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="registry">The request handler registry.</param>
    /// <param name="request">The request to be handled.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are propagated via TaskCompletionSource")]
    public async Task<TResponse> SendAsync<TResponse>(
        SingleRequestHandlerRegistry registry, ICalmRequest<TResponse> request,
        string memberName, string filePath, int lineNumber, CancellationToken token)
    {
        var requestType = request.GetType();
        var requestHandler = registry.GetHandler(requestType);

        var currentState = CalmContext.CurrentState;
        if (currentState is not null)
        {
            // Nested: Direct execution on engine thread
            try
            {
                return await requestHandler.HandleAsync<TResponse>(request, token).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                var unwrapped = CalmExceptionHelper.Unwrap(ex);
                _logger?.Error(unwrapped, "Unhandled exception while executing request handler.");
                ErrorObserver?.OnUnhandledException(unwrapped);
                throw;
            }
        }

        // Root call
        if (!Scheduler.ScheduleRequired)
        {
            return await ExecuteRootUoWAsync(() => requestHandler.HandleAsync<TResponse>(request, token))
                .ConfigureAwait(true);
        }

        // Outside: Schedule and Wait
        var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        await Scheduler.ScheduleAsync(async passedToken =>
        {
            try
            {
                var res = await ExecuteRootUoWAsync(() => requestHandler.HandleAsync<TResponse>(request, passedToken))
                    .ConfigureAwait(true);
                tcs.TrySetResult(res);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(CalmExceptionHelper.Unwrap(ex));
            }
        }, memberName, filePath, lineNumber, token).ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes an message to all registered handlers for the message type.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="registry">The request handler registry.</param>
    /// <param name="message">The message to be handled.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>true if scheduled; false if deferred.</returns>
    /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
    public bool Publish<TMessage>(
        MultipleMessageHandlerRegistry registry, TMessage message,
        string memberName, string filePath, int lineNumber, CancellationToken token)
        where TMessage : ICalmMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var metadata = CalmMessageMetadata.Get(typeof(TMessage));

        // If we are within a UoW and NOT an immediate event, defer publication to the outbox.
        if (!metadata.Immediate && CalmContext.CurrentState is { } state)
        {
            _ = state.Outbox.TryAdd(() => ExecuteMultiAsync(registry, message, metadata, token));
            return false;
        }

        // No UoW exists or it's an immediate event. Schedule a new root transaction on the engine thread.
        // This is always "Post only" (Fire-and-forget from the caller's perspective).
        Scheduler.Schedule(_ => ExecuteRootUoWAsync(() => ExecuteMultiAsync(registry, message, metadata, token)),
            memberName, filePath, lineNumber, token);
        return true;
    }

    /// <summary>
    /// Publishes an message to all registered handlers for the message type and waits for completion.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="registry">The request handler registry.</param>
    /// <param name="message">The message to be handled.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>true if scheduled; false if deferred.</returns>
    /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are propagated via TaskCompletionSource")]
    public async Task<bool> PublishAsync<TMessage>(
        MultipleMessageHandlerRegistry registry, TMessage message,
        string memberName, string filePath, int lineNumber, CancellationToken token)
        where TMessage : ICalmMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var metadata = CalmMessageMetadata.Get(typeof(TMessage));

        // If we are within a UoW and NOT an immediate event, defer publication to the outbox.
        if (!metadata.Immediate && CalmContext.CurrentState is { } state)
        {
            _ = state.Outbox.TryAdd(() => ExecuteMultiAsync(registry, message, metadata, token));
            return false;
        }

        // No UoW exists or it's an immediate event. Schedule and Wait
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await Scheduler.ScheduleAsync(async ct =>
        {
            try
            {
                await ExecuteRootUoWAsync(() => ExecuteMultiAsync(registry, message, metadata, ct))
                    .ConfigureAwait(true);
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(CalmExceptionHelper.Unwrap(ex));
            }
        }, memberName, filePath, lineNumber, token).ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes message handlers sequentially on the engine thread.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <param name="registry">The request handler registry.</param>
    /// <param name="message">The message.</param>
    /// <param name="metadata">The event metadata.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the completion of all handlers.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are forwarded to observer to allow other handlers to run")]
    private async Task ExecuteMultiAsync<TMessage>(
        MultipleMessageHandlerRegistry registry, TMessage message,
        CalmMessageMetadata metadata, CancellationToken token)
        where TMessage : ICalmMessage
    {
        var messageType = typeof(TMessage);

        if (!metadata.SuppressLog)
        {
            _logger?.DispatchInfo(LogLevel.Trace, "Starting handlers execution.", "PublishAsync", message);
        }

        if (!registry.HasHandlers(messageType))
        {
            _logger?.NoHandlersRegistered(LogLevel.Warning, messageType);
            return;
        }

        foreach (var handler in registry.GetHandlers(messageType))
        {
            try
            {
                await handler.HandleAsync(message, token).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                var unwrapped = CalmExceptionHelper.Unwrap(ex);
                _logger?.Error(unwrapped, "Unhandled exception while executing event handler.");
                ErrorObserver?.OnUnhandledException(unwrapped);
                // Event handlers are independent; one failure doesn't stop others within the same event.
            }
        }

        if (!metadata.SuppressLog)
        {
            _logger?.DispatchInfo(LogLevel.Trace, "Finished handlers execution.", "PublishAsync", message);
        }
    }

    /// <summary>
    /// Executes the specified function as a root transaction, managing the outbox lifecycle.
    /// </summary>
    /// <param name="funcAsync">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are propagated via TaskCompletionSource")]
    public async Task ExecuteRootUoWAsync(Func<Task> funcAsync)
    {
        var state = new CalmExecutionContextState();
        CalmContext.SetCurrentState(state);
        try
        {
            await funcAsync().ConfigureAwait(true);
            await ExecuteOutboxAsync(state.Outbox).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            // If the root function fails, the outbox is discarded (Atomic failure).
            throw CalmExceptionHelper.Unwrap(ex);
        }
        finally
        {
            CalmContext.SetCurrentState(null);
        }
    }

    /// <summary>
    /// Executes the specified function as a root transaction, managing the outbox lifecycle.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="funcAsync">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Exceptions are propagated via TaskCompletionSource")]
    public async Task<T> ExecuteRootUoWAsync<T>(Func<Task<T>> funcAsync)
    {
        var state = new CalmExecutionContextState();
        CalmContext.SetCurrentState(state);
        try
        {
            var result = await funcAsync().ConfigureAwait(true);
            await ExecuteOutboxAsync(state.Outbox).ConfigureAwait(true);
            return result;
        }
        catch (Exception ex)
        {
            // If the root function fails, the outbox is discarded (Atomic failure).
            throw CalmExceptionHelper.Unwrap(ex);
        }
        finally
        {
            CalmContext.SetCurrentState(null);
        }
    }

    /// <summary>
    /// Executes the handler in the Outbox.
    /// </summary>
    /// <param name="outbox">The outbox.</param>
    /// <returns>The result of the function.</returns>
    private async Task ExecuteOutboxAsync(IProducerConsumerCollection<Func<Task>> outbox)
    {
        var count = outbox.Count;
        if (count > 0)
        {
            _logger?.FlushAllDeferredEvents(LogLevel.Trace, count);

            // Loop until the outbox is truly empty to handle chained events.
            while (outbox.TryTake(out var func))
            {
                await func().ConfigureAwait(true);
            }
        }
    }

    /// <summary>
    /// Logs the dispatch information.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="busMessage">The Calm bus message object.</param>
    /// <param name="busMessageType">The type of <paramref name="busMessage"/>.</param>
    /// <param name="methodName">The name of the method performing the publication.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    public void LogDispatchInfo(string message, object busMessage, Type busMessageType, string methodName,
        string memberName, string filePath, int lineNumber)
    {
        var metadata = CalmMessageMetadata.Get(busMessageType);
        if (metadata.SuppressLog)
        {
            return;
        }
        _logger?.DispatchInfo(LogLevel.Trace, message, methodName, busMessage,
            memberName, filePath, lineNumber);
    }
}
