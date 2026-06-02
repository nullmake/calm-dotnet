using Calm.Core;

namespace Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;

/// <summary>
/// A test event for attribute-based handler testing.
/// </summary>
/// <param name="Message">The event message.</param>
internal sealed record TestEvent(string Message) : ICalmEvent;
