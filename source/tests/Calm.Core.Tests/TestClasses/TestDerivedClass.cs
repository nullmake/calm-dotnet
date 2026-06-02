using Microsoft.Extensions.Logging;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// A class that derived from TestClass.
/// </summary>
/// <param name="engine">The Calm engine.</param>
/// <param name="logger">The test output helper used to write test output during execution.</param>
internal sealed class TestDerivedClass(ICalm engine, ILogger logger) : TestClass(engine, logger)
{
}
