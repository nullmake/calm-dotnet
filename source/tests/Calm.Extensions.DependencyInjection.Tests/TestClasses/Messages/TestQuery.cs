using Calm.Core;

namespace Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;

/// <summary>
/// A test request for DI integration.
/// </summary>
/// <param name="Data">The query message.</param>
internal sealed record TestQuery(string Data) : ICalmQuery<string>;
