using Calm.Core;

namespace Sample04.Models.Bus.Events;

internal sealed record ProcessResourceUpdatedEvent : ICalmEvent
{
    public required double CpuUsage { get; init; }
    public required long PrivateBytes { get; init; }
    public required long WorkingSet { get; init; }
    public required long VirtualMermory { get; init; }
    public required int HandleCount { get; init; }
    public required long GcHeapSize { get; init; }
}
