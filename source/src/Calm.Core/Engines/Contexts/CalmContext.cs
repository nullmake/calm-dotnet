namespace Calm.Core.Engines.Contexts;

/// <summary>
/// Provides access to the CALM execution context, including the current task metadata.
/// This uses <see cref="AsyncLocal{T}"/> to ensure the context flows across asynchronous boundaries
/// (e.g., across await points and thread switches), allowing code running on the engine thread
/// to identify the current task it is processing.
/// </summary>
internal static class CalmContext
{
    /// <summary>
    /// Tracks the metadata for the current task.
    /// The value is preserved across asynchronous calls within the same logical execution flow.
    /// </summary>
    private static readonly AsyncLocal<CalmTaskInfo?> _currentTask = new();

    /// <summary>
    /// Tracks the internal execution state for the current context.
    /// </summary>
    private static readonly AsyncLocal<CalmExecutionContextState?> _currentState = new();

    /// <summary>
    /// Gets the metadata for the task currently being executed by the CALM engine.
    /// Returns <see langword="null"/> if the caller is not running within a managed CALM task.
    /// </summary>
    public static CalmTaskInfo? CurrentTask => _currentTask.Value;

    /// <summary>
    /// Gets the internal execution state for the current context.
    /// This state is used for internal engine diagnostics and state management.
    /// </summary>
    internal static CalmExecutionContextState? CurrentState => _currentState.Value;

    /// <summary>
    /// Sets the current task metadata for the current asynchronous flow.
    /// This is intended for internal engine use only.
    /// </summary>
    /// <param name="taskInfo">The task metadata to set.</param>
    internal static void SetCurrentTask(CalmTaskInfo? taskInfo)
    {
        _currentTask.Value = taskInfo;
    }

    /// <summary>
    /// Sets the current internal execution state for the current asynchronous flow.
    /// This is intended for internal engine use only.
    /// </summary>
    /// <param name="state">The execution state to set.</param>
    internal static void SetCurrentState(CalmExecutionContextState? state)
    {
        _currentState.Value = state;
    }
}
