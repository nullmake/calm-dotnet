namespace Calm.Core.Engines.SynchronizationContexts;

/// <summary>
/// Defines methods for dispatching messages to a synchronization context, supporting both synchronous and
/// asynchronous operations.
/// </summary>
internal interface ICalmSynchronizationContextDispatcher
{
    /// <summary>
    /// Dispatches a synchronous message to a synchronization context.
    /// </summary>
    /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
    /// <param name="state">The object passed to the delegate.</param>
    void Send(SendOrPostCallback d, object state);

    /// <summary>
    /// Dispatches an asynchronous message to a synchronization context.
    /// </summary>
    /// <param name="d">The System.Threading.SendOrPostCallback delegate to call.</param>
    /// <param name="state">The object passed to the delegate.</param>
    void Post(SendOrPostCallback d, object state);
}
