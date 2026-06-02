#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides metadata for a task being executed within the CALM engine.
/// This allows rich diagnostics and tracking of asynchronous operations.
/// </summary>
/// <param name="Id">The unique ID for the task instance.</param>
/// <param name="Name">The name of the operation (e.g., caller member name).</param>
/// <param name="FilePath">The source file where the task was scheduled.</param>
/// <param name="LineNumber">The line number where the task was scheduled.</param>
/// <param name="Timestamp">The UTC timestamp when the task was scheduled.</param>
public record CalmTaskInfo(
    Guid Id,
    string Name,
    string FilePath,
    int LineNumber,
    DateTimeOffset Timestamp);
