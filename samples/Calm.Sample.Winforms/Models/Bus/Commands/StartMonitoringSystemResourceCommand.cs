using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Commands;

/// <summary>
/// The command to start monitoring the application's system resources.
/// </summary>
/// <param name="SamplingPeriod">The sampling period.</param>
/// <remarks>
/// Returns true if the monitoring has started; false if already monitoring.
/// </remarks>
internal sealed record StartMonitoringSystemResourceCommand(TimeSpan SamplingPeriod) : ICalmCommand<bool>
{
}
