using Calm.Core;

namespace Sample04.Models.Bus.Commands;

internal sealed record ChangeSamplingPeriodCommand(TimeSpan Period) : ICalmCommand
{
}
