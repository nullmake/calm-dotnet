namespace Calm.Core.Engines;

/// <summary>
/// An internal representation of a unit of work scheduled in the CalmPump.
/// </summary>
/// <param name="onExecuteAsync">The actual delegate to execute.</param>
/// <param name="metadata">Rich tracking information for the task.</param>
internal sealed class CalmTask(
    Func<Task?> onExecuteAsync,
    CalmTaskInfo metadata)
{
    /// <summary>
    /// Gets or sets the actual delegate to execute.
    /// </summary>
    public Func<Task?> OnExecuteAsync { get; set; } = onExecuteAsync;

    /// <summary>
    /// Gets the rich tracking information for the task.
    /// </summary>
    public CalmTaskInfo Metadata { get; } = metadata;

    /// <summary>
    /// Indicates whether a slot in the logical queue was reserved for this task.
    /// </summary>
    public bool IsCapacityReserved { get; set; } = true;
}
