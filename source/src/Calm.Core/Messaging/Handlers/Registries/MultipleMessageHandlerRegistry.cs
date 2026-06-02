using Calm.Core.Messaging.Handlers.Collections;
using System.Collections;

namespace Calm.Core.Messaging.Handlers.Registries;

/// <summary>
/// Manages multiple handlers per message type.
/// </summary>
internal sealed class MultipleMessageHandlerRegistry : IEnumerable<IMessageHandler>
{
    /// <summary>
    /// Storage for handlers.
    /// </summary>
    private readonly MultipleHandlerCollection<IMessageHandler> _messageHandlersCollection = [];

    /// <summary>
    /// Removes all message handlers.
    /// </summary>
    public void Clear()
        => _messageHandlersCollection.Clear();

    /// <summary>
    /// Registers a message handler.
    /// </summary>
    /// <param name="messageHandler">The message handler to register.</param>
    public void Register(IMessageHandler messageHandler)
        => _messageHandlersCollection.Add(messageHandler.MessageType, messageHandler);

    /// <summary>
    /// Unregisters a handler by delegate match.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="method">The method to be removed.</param>
    /// <returns>true if item is successfully removed; otherwise, false.
    /// This method also returns false if item was not found</returns>
    public bool Unregister(Type messageType, Delegate method)
        => _messageHandlersCollection.Remove(messageType, method);

    /// <summary>
    /// Gets all handlers for a message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>All handlers for the message type.</returns>
    public IEnumerable<IMessageHandler> GetHandlers(Type messageType)
        => _messageHandlersCollection.EnumerateHandlers(messageType);

    /// <summary>
    /// Checks if any handlers are registered for a message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>true if handlers are registered; otherwise, false.</returns>
    public bool HasHandlers(Type messageType)
        => _messageHandlersCollection.HasHandlers(messageType);

    /// <inheritdoc/>
    public IEnumerator<IMessageHandler> GetEnumerator()
        => _messageHandlersCollection.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
