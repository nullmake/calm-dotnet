using System.Collections.Concurrent;

namespace Calm.Core.Engines.Contexts;

/// <summary>
/// Holds the internal state for the current CALM execution context.
/// This will be used for coordinating operations like outbox publishing.
/// </summary>
internal sealed class CalmExecutionContextState
{
    /// <summary>
    /// Gets the queue of deferred actions (e.g., event publications)
    /// to be executed upon successful completion of the current command.
    /// </summary>
    public IProducerConsumerCollection<Func<Task>> Outbox { get; } = new ConcurrentQueue<Func<Task>>();
}
