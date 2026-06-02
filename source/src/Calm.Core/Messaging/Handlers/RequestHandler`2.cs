using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// A handler that wraps a callback delegate for processing messages with response.
/// This class enforces Calm's core principles: asynchronous execution with mandatory cancellation support.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestHandler{TRequest, TResponse}"/> class.
/// </remarks>
/// <param name="callback">The callback handler to wrap. Must be a method marked
/// with <see cref="CalmHandlerAttribute"/>.</param>
/// <exception cref="InvalidOperationException">Thrown when the callback handler is not marked
/// with <see cref="CalmHandlerAttribute"/>.</exception>
internal sealed class RequestHandler<TRequest, TResponse>(
    Func<TRequest, CancellationToken, Task<TResponse>> callback)
    : Handler<Func<TRequest, CancellationToken, Task<TResponse>>>(callback), IRequestHandler,
    IEquatable<RequestHandler<TRequest, TResponse>>
    where TRequest : ICalmRequest<TResponse>
{
    /// <inheritdoc/>
    public Type RequestType => typeof(TRequest);

    /// <inheritdoc/>
    public Type ResponseType => typeof(TResponse);

    /// <summary>
    /// Processes the specified request asynchronously and returns a response.
    /// </summary>
    /// <param name="request">The request to be handled.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A <see cref="Task{TResponse}"/> representing the result of the request processing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken token = default)
        => Callback(request, token);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Task<T> IRequestHandler.HandleAsync<T>(object request, CancellationToken token)
        => (Task<T>)(object)HandleAsync((TRequest)request, token);

    #region IEquatable<>
    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as RequestHandler<TRequest, TResponse>);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hashCode = 782_066_028;
        hashCode = (hashCode * -1_521_134_295) + EqualityComparer<Func<TRequest, CancellationToken, Task<TResponse>>>
            .Default.GetHashCode(Callback);
        hashCode = (hashCode * -1_521_134_295) + StringComparer.Ordinal.GetHashCode(Name);
        hashCode = (hashCode * -1_521_134_295) + EqualityComparer<Type>.Default.GetHashCode(RequestType);
        hashCode = (hashCode * -1_521_134_295) + EqualityComparer<Type>.Default.GetHashCode(ResponseType);
        return hashCode;
    }

    /// <inheritdoc/>
    public bool Equals([NotNullWhen(true)] RequestHandler<TRequest, TResponse>? other)
        => other is RequestHandler<TRequest, TResponse> handler
            && EqualityComparer<Func<TRequest, CancellationToken, Task<TResponse>>>
                .Default.Equals(Callback, handler.Callback)
            && string.Equals(Name, handler.Name, StringComparison.Ordinal)
            && EqualityComparer<Type>.Default.Equals(RequestType, handler.RequestType)
            && EqualityComparer<Type>.Default.Equals(ResponseType, handler.ResponseType);
    #endregion
}
