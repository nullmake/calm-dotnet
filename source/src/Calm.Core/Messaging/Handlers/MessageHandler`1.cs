using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// A handler that wraps a callback delegate for processing messages.
/// This class enforces Calm's core principles: asynchronous execution with mandatory cancellation support.
/// </summary>
/// <typeparam name="TMessage">The type of the message.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="MessageHandler{TMessage}"/> class.
/// </remarks>
/// <param name="callback">The callback handler. Must be a method marked
/// with <see cref="CalmHandlerAttribute"/>.</param>
/// <exception cref="InvalidOperationException">Thrown when the callback handler is not marked
/// with <see cref="CalmHandlerAttribute"/>.</exception>
internal sealed class MessageHandler<TMessage>(Func<TMessage, CancellationToken, Task> callback)
    : Handler<Func<TMessage, CancellationToken, Task>>(callback), IMessageHandler,
    IEquatable<MessageHandler<TMessage>>
    where TMessage : ICalmMessage
{
    /// <inheritdoc/>
    public Type MessageType => typeof(TMessage);

    /// <summary>
    /// Processes the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to be handled.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task HandleAsync(TMessage message, CancellationToken token = default)
        => Callback(message, token);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task IMessageHandler.HandleAsync(object message, CancellationToken token)
        => HandleAsync((TMessage)message, token);

    #region IEquatable<>
    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as MessageHandler<TMessage>);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = -1_948_609_240;
        hashCode = (hashCode * -1_521_134_295) + EqualityComparer<Func<TMessage, CancellationToken, Task>>
            .Default.GetHashCode(Callback);
        hashCode = (hashCode * -1_521_134_295) + StringComparer.Ordinal.GetHashCode(Name);
        hashCode = (hashCode * -1_521_134_295) + EqualityComparer<Type>.Default.GetHashCode(MessageType);
        return hashCode;
    }

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] MessageHandler<TMessage>? other)
        => other is MessageHandler<TMessage> handler
            && EqualityComparer<Func<TMessage, CancellationToken, Task>>
                .Default.Equals(Callback, handler.Callback)
            && string.Equals(Name, handler.Name, StringComparison.Ordinal)
            && EqualityComparer<Type>.Default.Equals(MessageType, handler.MessageType);
    #endregion
}
