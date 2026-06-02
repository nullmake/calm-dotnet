using Calm.Core.Messaging.Handlers;
using Calm.Core.Messaging.Handlers.Registries;
using Microsoft.Extensions.Logging;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// A concrete implementation of the query bus that handles query registration and dispatch.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmQueryBus"/> class.
/// </remarks>
/// <param name="busCore">The underlying execution engine.</param>
/// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
internal sealed class CalmQueryBus(CalmBusCore busCore, CalmBusLog? logger) : ICalmQueryBus
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
    /// Registry for query handlers.
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

        var requestHandler = RequestHandler.Create(calmHandlerInfo, instance);
        _requestHandlers.Register(requestHandler);
    }

    /// <inheritdoc/>
    void ICalmQueryBus.Register<TQuery, TResponse>(Func<TQuery, CancellationToken, Task<TResponse>> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            var requestHandler = new RequestHandler<TQuery, TResponse>(callback);

            _logger?.RegisteringHandler(LogLevel.Trace, requestHandler, memberName, filePath, lineNumber);

            _requestHandlers.Register(requestHandler);

            _logger?.RegisteredHandler(LogLevel.Information, requestHandler, memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    void ICalmQueryBus.Unregister(
        string memberName, string filePath, int lineNumber)
    {
        try
        {
            _logger?.WriteLine(LogLevel.Trace, "[Clear] Removing all query handlers.",
                memberName, filePath, lineNumber);

            _requestHandlers.Clear();

            _logger?.WriteLine(LogLevel.Information, "[Clear] Removed all query handlers.",
                memberName, filePath, lineNumber);
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Unregisters a callback handler for a specific query type.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The instance which has the method.</param>
    /// <exception cref="ArgumentNullException">The calmHandlerInfo parameter is null.</exception>
    internal void Unregister(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        ArgumentNullException.ThrowIfNull(calmHandlerInfo);

        var requestType = calmHandlerInfo.ParameterType;
        var method = calmHandlerInfo.CreateMethod(instance);
        _requestHandlers.Unregister(requestType, method);
    }

    /// <inheritdoc/>
    void ICalmQueryBus.Unregister<TQuery, TResponse>(
        Func<TQuery, CancellationToken, Task<TResponse>> callback,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(callback);

        try
        {
            _logger?.UnregisteringHandler(LogLevel.Trace, callback, typeof(TQuery),
                memberName, filePath, lineNumber);

            if (_requestHandlers.Unregister(typeof(TQuery), callback))
            {
                _logger?.UnregisteredHandler(LogLevel.Information, callback, typeof(TQuery),
                    memberName, filePath, lineNumber);
            }
            else
            {
                _logger?.NoHandlersRegistered(LogLevel.Trace, typeof(TQuery));
            }
        }
        catch (CalmException ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IReadOnlyRequestHandler> EnumerateRequestHandler()
        => _requestHandlers.AsEnumerable<IReadOnlyRequestHandler>();

    /// <inheritdoc/>
    Task<TResponse> ICalmQueryBus.SendAsync<TResponse>(ICalmQuery<TResponse> query,
        string memberName, string filePath, int lineNumber)
        => ((ICalmQueryBus)this).SendAsync(query, CancellationToken.None, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    Task<TResponse> ICalmQueryBus.SendAsync<TResponse>(ICalmQuery<TResponse> query, CancellationToken token,
        string memberName, string filePath, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(query);

        LogSend(query, query.GetType(), "ICalmQueryBus.SendAsync", memberName, filePath, lineNumber);

        return _bus.SendAsync(_requestHandlers, query, memberName, filePath, lineNumber, token);
    }

    /// <summary>
    /// Logs the sending of an query.
    /// </summary>
    /// <param name="query">The query to be send.</param>
    /// <param name="queryType">The type of the query.</param>
    /// <param name="methodName">The name of the method performing the publication.</param>
    /// <param name="memberName">The caller member name.</param>
    /// <param name="filePath">The caller file path.</param>
    /// <param name="lineNumber">The caller line number.</param>
    private void LogSend(object query, Type queryType, string methodName,
        string memberName, string filePath, int lineNumber)
        => _bus.LogDispatchInfo("Starting.", query, queryType, methodName,
            memberName, filePath, lineNumber);
}
