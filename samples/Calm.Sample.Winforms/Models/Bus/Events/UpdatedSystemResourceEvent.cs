using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.Application;

namespace Calm.Sample.Winforms.Models.Bus.Events;

/// <summary>
/// An event for updated system resources of the current application.
/// </summary>
/// <param name="Sample">The performance data sample for the current application.</param>
[CalmSuppressLog]
internal sealed record UpdatedSystemResourceEvent(ProcessPerformanceSample Sample) : ICalmEvent
{
}
