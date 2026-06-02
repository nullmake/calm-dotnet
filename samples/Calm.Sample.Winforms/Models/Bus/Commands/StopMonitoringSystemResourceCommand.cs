using Calm.Core;

namespace Calm.Sample.Winforms.Models.Bus.Commands;

/// <summary>
/// The command to stop monitoring the application's system resources.
/// </summary>
internal sealed record StopMonitoringSystemResourceCommand : ICalmCommand
{
}
