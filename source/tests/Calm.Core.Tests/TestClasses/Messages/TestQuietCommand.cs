namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// A command that should have its logs suppressed.
/// </summary>
[CalmSuppressLog]
internal sealed record TestQuietCommand : ICalmCommand;
