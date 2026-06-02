namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// An event that should have its logs suppressed.
/// </summary>
[CalmSuppressLog]
internal sealed record TestQuietEvent : ICalmEvent;
