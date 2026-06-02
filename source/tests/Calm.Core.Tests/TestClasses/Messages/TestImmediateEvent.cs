namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// An event that should be published immediately.
/// </summary>
[CalmImmediate]
internal sealed record TestImmediateEvent : ICalmEvent;
