using Calm.Core.Messaging.Handlers.Collections;
using System.Collections;

namespace Calm.Core.Messaging.Handlers.Registries;

/// <summary>
/// Manages single handlers per request handler.
/// </summary>
internal sealed class SingleRequestHandlerRegistry : IEnumerable<IRequestHandler>
{
    /// <summary>
    /// Storage for handlers.
    /// </summary>
    private readonly SingleHandlerCollection<IRequestHandler> _requestHandlers = [];

    /// <summary>
    /// Removes all message handlers.
    /// </summary>
    public void Clear()
        => _requestHandlers.Clear();

    /// <summary>
    /// Registers a handler.
    /// </summary>
    /// <param name="requestHandler">The delegate handler to register.</param>
    public void Register(IRequestHandler requestHandler)
        => _requestHandlers.Add(requestHandler.RequestType, requestHandler);

    /// <summary>
    /// Unregisters a handler by request type.
    /// </summary>
    /// <param name="requestType">The type of request.</param>
    /// <param name="method">The method to be removed.</param>
    /// <returns>true if item is successfully removed; otherwise, false.
    /// This method also returns false if item was not found</returns>
    public bool Unregister(Type requestType, Delegate method)
        => _requestHandlers.Remove(requestType, method);

    /// <summary>
    /// Gets a handler by message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>The handler, or null if not found.</returns>
    public IRequestHandler GetHandler(Type messageType)
        => _requestHandlers.GetValue(messageType);

    /// <summary>
    ///  Attempts to get the delegate handler associated with the request type.
    /// </summary>
    /// <param name="requestType">Type of the request type.</param>
    /// <param name="requestHandler">The delegate handler associated with the request type.</param>
    /// <returns>true if the delegate handler was found otherwise, false.</returns>
    public bool TryGetHandler(Type requestType, out IRequestHandler requestHandler)
        => _requestHandlers.TryGetValue(requestType, out requestHandler);

    /// <inheritdoc/>
    public IEnumerator<IRequestHandler> GetEnumerator()
        => _requestHandlers.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
