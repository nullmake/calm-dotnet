using Calm.Core;

namespace Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;

/// <summary>
/// A test command for attribute-based handler testing.
/// </summary>
/// <param name="Data">The command message.</param>
internal sealed record TestCommand(string Data) : ICalmCommand;
