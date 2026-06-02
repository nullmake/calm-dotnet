using System.Collections;
using System.Collections.Concurrent;

namespace Calm.Core.Messaging.Handlers.Collections;

/// <summary>
///  Represents a thread-safe collection of message type/handler object pairs that can be accessed by
///  multiple threads concurrently.
/// </summary>
/// <typeparam name="THandler">The type of handler object.</typeparam>
internal sealed class MultipleHandlerCollection<THandler> : IEnumerable<THandler>
    where THandler : notnull, IHandler
{
    /// <summary>
    /// Storage for handlers.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<THandler, byte>> _handlersCollection = new();

    /// <summary>
    /// Lock object for synchronizing registration and removal.
    /// </summary>
#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _syncLock = new();
#else
    private readonly object _syncLock = new();
#endif

    /// <summary>
    /// Removes all message handlers.
    /// </summary>
    public void Clear()
    {
        _handlersCollection.Clear();
    }

    /// <summary>
    ///  Attempts to add the specified message type and handler object.
    /// </summary>
    /// <param name="messageType">The message type of the element to add.</param>
    /// <param name="handler">The value of the element to add. </param>
    /// <exception cref="CalmHandlerAlreadyRegisteredException">The message type already added.</exception>
    /// <exception cref="CalmNullHandlerRegistationException">The parameter handler given is null.</exception>
    public void Add(Type messageType, THandler handler)
    {
        if (handler is null)
        {
            throw new CalmNullHandlerRegistationException(messageType);
        }

        lock (_syncLock)
        {
            var handlers = _handlersCollection.GetOrAdd(messageType, _ => new ConcurrentDictionary<THandler, byte>());
            handlers.TryAdd(handler, 0);
        }
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified message type.
    /// </summary>
    /// <param name="messageType">The message type of the element to remove and return.</param>
    /// <param name="method">The method to be removed.</param>
    /// <returns>true if item is successfully removed; otherwise, false.
    /// This method also returns false if item was not found</returns>
    public bool Remove(Type messageType, Delegate method)
    {
        lock (_syncLock)
        {
            if (!_handlersCollection.TryGetValue(messageType, out var handlers))
            {
                return false;
            }

            foreach (var key in handlers.Keys)
            {
                if (key.Matches(method) && handlers.TryRemove(key, out _))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if any handlers are registered for a message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>true if handlers are registered; otherwise, false.</returns>
    public bool HasHandlers(Type messageType)
        => _handlersCollection.TryGetValue(messageType, out var handlers) && !handlers.IsEmpty;

    /// <summary>
    /// Returns an enumerable collection of the handlers for the message type.
    /// </summary>
    /// <param name="messageType">Type of the message.</param>
    /// <returns>An enumerable collection of the handlers for the message type.</returns>
    public IEnumerable<THandler> EnumerateHandlers(Type messageType)
    {
        if (_handlersCollection.TryGetValue(messageType, out var handlers))
        {
            return handlers.Keys;
        }
        return [];
    }

    /// <inheritdoc/>
    public IEnumerator<THandler> GetEnumerator()
        => _handlersCollection.Values.SelectMany(kv => kv.Keys).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
