namespace Calm.Core.Diagnostics;

/// <summary>
/// Identifies the source module of a log message.
/// </summary>
internal enum CalmLogModule
{
    /// <summary>
    /// CalmLog
    /// </summary>
    CalmLog = 1,

    /// <summary>
    /// CalmPump
    /// </summary>
    CalmPump = 2,

    /// <summary>
    /// CalmBus
    /// </summary>
    CalmBus = 3,
}
