using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides access to the OpenTelemetry instrumentation for CALM.
/// </summary>
public static class CalmTelemetry
{
    /// <summary>
    /// Represents the name of the Calm.Core service.
    /// </summary>
    internal const string ServiceName = "Calm.Core";

    /// <summary>
    /// Represents the assembly name for the CalmTelemetry type.
    /// </summary>
    internal static readonly AssemblyName AssemblyName = typeof(CalmTelemetry).Assembly.GetName();

    /// <summary>
    /// Gets the version string of the assembly, or null if the version is unavailable.
    /// </summary>
    internal static readonly string? Version = AssemblyName.Version?.ToString();

    /// <summary>
    /// The ActivitySource for CALM traces.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, Version);

    /// <summary>
    /// The Meter for CALM metrics.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, Version);

    /// <summary>
    /// Measures the number of items currently waiting in the engine queue.
    /// </summary>
    internal static readonly UpDownCounter<long> QueueDepth = Meter.CreateUpDownCounter<long>(
        "calm.engine.queue_depth",
        description: "The number of items waiting in the engine channel.");

    /// <summary>
    /// Measures the time taken to process a single message/action.
    /// </summary>
    internal static readonly Histogram<double> ProcessingDuration = Meter.CreateHistogram<double>(
        "calm.engine.processing_duration",
        unit: "ms",
        description: "The duration of message processing on the engine thread.");

    /// <summary>
    /// Counts the total number of messages processed.
    /// </summary>
    internal static readonly Counter<long> MessagesProcessed = Meter.CreateCounter<long>(
        "calm.engine.messages_processed",
        description: "The total number of messages processed by the engine.");

    /// <summary>
    /// Enriches the provided activity with task metadata.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="task">The task metadata.</param>
    /// <exception cref="ArgumentNullException">The task parameter is null.</exception>
    public static void EnrichActivity(Activity? activity, CalmTaskInfo task)
    {
        if (activity is null)
        {
            return;
        }
        ArgumentNullException.ThrowIfNull(task);

        activity
            .SetTag("calm.task.id", task.Id)
            .SetTag("calm.task.name", task.Name)
            .SetTag("calm.task.file", task.FilePath)
            .SetTag("calm.task.line", task.LineNumber);
    }
}
