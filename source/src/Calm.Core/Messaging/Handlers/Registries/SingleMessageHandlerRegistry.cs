using Calm.Core.Messaging.Handlers.Collections;
using System.Collections;

namespace Calm.Core.Messaging.Handlers.Registries;

/// <summary>
/// Manages single handlers per message type.
/// </summary>
internal sealed class SingleMessageHandlerRegistry : IEnumerable<IMessageHandler>
{
    /// <summary>
    /// Storage for handlers.
    /// </summary>
    private readonly SingleHandlerCollection<IMessageHandler> _messageHandlers = [];

    /// <summary>
    /// Removes all message handlers.
    /// </summary>
    public void Clear()
        => _messageHandlers.Clear();

    /// <summary>
    /// Registers a handler.
    /// </summary>
    /// <param name="messageHandler">The handler to register.</param>
    public void Register(IMessageHandler messageHandler)
        => _messageHandlers.Add(messageHandler.MessageType, messageHandler);

    /// <summary>
    /// Unregisters a handler by message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="method">The method to be removed.</param>
    /// <returns>true if item is successfully removed; otherwise, false.</returns>
    public bool Unregister(Type messageType, Delegate method)
        => _messageHandlers.Remove(messageType, method);

    /// <summary>
    /// Gets a handler by message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>The handler, or null if not found.</returns>
    public IMessageHandler GetHandler(Type messageType)
        => _messageHandlers.GetValue(messageType);

    /// <summary>
    ///  Attempts to get the delegate handler associated with the message type.
    /// </summary>
    /// <param name="messageType">Type of the message type.</param>
    /// <param name="messageHandler">The delegate handler associated with the request type.</param>
    /// <returns>true if the delegate handler was found otherwise, false.</returns>
    public bool TryGetHandler(Type messageType, out IMessageHandler messageHandler)
        => _messageHandlers.TryGetValue(messageType, out messageHandler);

    /// <inheritdoc/>
    public IEnumerator<IMessageHandler> GetEnumerator()
        => _messageHandlers.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
