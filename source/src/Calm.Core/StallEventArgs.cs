using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Calm.Core;

/// <summary>
/// Provides data for the stall events.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StallEventArgs"/> class.
/// </remarks>
/// <param name="task">The metadata of the task that stalled, or null if unknown or recovered.</param>
/// <param name="duration">The duration the task has occupied the worker thread.</param>
public sealed class StallEventArgs(CalmTaskInfo? task = null, TimeSpan? duration = null) : EventArgs
{
    /// <summary>
    /// Gets the metadata of the task that stalled.
    /// </summary>
    public CalmTaskInfo? Task { get; } = task;

    /// <summary>
    /// Gets the duration the task has occupied the worker thread.
    /// </summary>
    public TimeSpan? Duration { get; } = duration;

    /// <inheritdoc/>
    [SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity",
       Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    [SuppressMessage("Usage", "MA0001:StringComparison is missing",
       Justification = "Because net472 and netstandard2.0 do not support StringComparison")]
    public override string ToString()
    {
        var taskString = Task?.ToString()
            .Replace(nameof(CalmTaskInfo), "")
            .Replace(" = ", "=")
            .Trim() ?? "(none)";
        return string.Format(CultureInfo.InvariantCulture, "{{ Task={0}, Duration={1} }}", taskString, Duration);
    }
}
