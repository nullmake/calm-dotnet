namespace Calm.Core.Engines.SynchronizationContexts;

/// <summary>
/// A <see cref="SynchronizationContext"/> implementation that delegates work to a <see cref="ICalmPump"/>.
/// Ensures that the execution environment is consistent with the engine's single-threaded model.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmSynchronizationContext"/> class.
/// </remarks>
/// <param name="dispatcher">Defines methods for dispatching messages to a synchronization context</param>
internal sealed class CalmSynchronizationContext(ICalmSynchronizationContextDispatcher dispatcher)
    : SynchronizationContext
{
    /// <summary>
    /// Provides access to the dispatcher used for synchronizing operations within the current context.
    /// </summary>
    private readonly ICalmSynchronizationContextDispatcher _dispatcher = dispatcher;

    /// <summary>
    /// Dispatches an asynchronous message to the message pump context.
    /// </summary>
    /// <param name="d">The delegate to call.</param>
    /// <param name="state">The object passed to the delegate.</param>
    public override void Post(SendOrPostCallback d, object? state)
        => _dispatcher.Post(d, state ?? new object());

    /// <summary>
    /// Dispatches a synchronous message to the message pump context.
    /// </summary>
    /// <param name="d">The delegate to call.</param>
    /// <param name="state">The object passed to the delegate.</param>
    public override void Send(SendOrPostCallback d, object? state)
        => _dispatcher.Send(d, state ?? new object());

    /// <summary>
    /// Creates a copy of the synchronization context.
    /// </summary>
    /// <returns>A new <see cref="SynchronizationContext"/> object.</returns>
    public override SynchronizationContext CreateCopy() => this;
}
