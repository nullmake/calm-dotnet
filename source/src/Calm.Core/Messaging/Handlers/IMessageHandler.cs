namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// A non-generic interface for delegate handlers without response.
/// </summary>
internal interface IMessageHandler : IReadOnlyMessageHandler, IHandler
{
    /// <summary>
    /// Handles the message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(object message, CancellationToken token = default);
}
