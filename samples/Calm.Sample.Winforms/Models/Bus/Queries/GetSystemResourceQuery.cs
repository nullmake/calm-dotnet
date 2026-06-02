using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.Application;

namespace Calm.Sample.Winforms.Models.Bus.Queries;

/// <summary>
/// A query to get the system resources of the current application.
/// </summary>
internal sealed record GetSystemResourceQuery : ICalmQuery<ProcessPerformanceSample>
{
}
