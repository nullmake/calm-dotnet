namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// A non-generic interface for delegate handlers with response
/// This is a marker interface for type-safe handler management.
/// </summary>
internal interface IRequestHandler : IReadOnlyRequestHandler, IHandler
{
    /// <summary>
    /// Handles the request asynchronously.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="request">The request to handle.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>Represents an asynchronous operation that can return a response.</returns>
    Task<T> HandleAsync<T>(object request, CancellationToken token = default);
}
