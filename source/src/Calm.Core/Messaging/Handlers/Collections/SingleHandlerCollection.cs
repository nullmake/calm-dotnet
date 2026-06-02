using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Messaging.Handlers.Collections;

/// <summary>
///  Represents a thread-safe collection of message type/handler object pairs
///  that can be accessed by multiple threads concurrently.
/// </summary>
/// <typeparam name="THandler">The type of handler object.</typeparam>
internal sealed class SingleHandlerCollection<THandler> : IEnumerable<THandler>
    where THandler : notnull, IHandler
{
    /// <summary>
    /// Storage for handlers.
    /// </summary>
    private readonly ConcurrentDictionary<Type, THandler> _handlers = new();

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
        _handlers.Clear();
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
            if (!_handlers.TryAdd(messageType, handler))
            {
                if (_handlers.TryGetValue(messageType, out var existing)
                    && !existing.Matches(handler.Callback))
                {
                    throw new CalmHandlerAlreadyRegisteredException(messageType, handler.Name, existing.Name);
                }
            }
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
            if (_handlers.TryGetValue(messageType, out var handler)
                && handler.Matches(method))
            {
                return _handlers.TryRemove(messageType, out _);
            }
        }

        return false;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified message type.
    /// </summary>
    /// <param name="messageType">The message type of the handler object to get.</param>
    /// <param name="handler">When this method returns, contains the handler object,
    /// or the default value of the type if the operation failed.</param>
    /// <returns>true if the message type was found otherwise, false.</returns>
    public bool TryGetValue(Type messageType, [MaybeNullWhen(false)] out THandler handler)
        => _handlers.TryGetValue(messageType, out handler);

    /// <summary>
    /// Gets the value associated with the specified message type.
    /// </summary>
    /// <param name="messageType">The message type of the handler object to get.</param>
    /// <returns>The handler object of the message type.</returns>
    /// <exception cref="CalmNoHandlerRegisteredException">the message type was not found.</exception>
    public THandler GetValue(Type messageType)
        => TryGetValue(messageType, out var handler)
            ? handler
            : throw new CalmNoHandlerRegisteredException(messageType);

    /// <inheritdoc/>
    public IEnumerator<THandler> GetEnumerator()
        => _handlers.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
