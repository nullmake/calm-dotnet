namespace Calm.Core.Engines;

/// <summary>
/// An internal representation of a unit of work scheduled in the CalmPump.
/// </summary>
/// <param name="OnExecuteAsync">The actual delegate to execute.</param>
/// <param name="Metadata">Rich tracking information for the task.</param>
internal sealed record CalmTask(
    Func<Task?> OnExecuteAsync,
    CalmTaskInfo Metadata)
{
    /// <summary>
    /// Indicates whether a slot in the logical queue was reserved for this task.
    /// </summary>
    public bool IsCapacityReserved { get; set; } = true;
}
