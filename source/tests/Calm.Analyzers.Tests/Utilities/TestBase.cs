using Calm.Core;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using SharedTestCode;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Analyzers.Tests.Utilities;

/// <summary>
/// The base class for all tests in this project, providing common setup and utilities.
/// </summary>
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",
    Justification = "Test classes and related classes must be public.")]
public abstract class TestBase
{
    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="level">The test output log level</param>
    protected TestBase(LogLevel level)
    {
        Logger = LoggerHelper.CreateLogger(level, GetType().Name);
    }

    /// <summary>
    /// Logs output at the start and end of a block.
    /// </summary>
    /// <param name="blockName">The block name.</param>
    /// <returns>The log output object.</returns>
    protected IDisposable BlockLog(string blockName)
    {
        Logger.LogInformation("({Name}) -->", blockName);
        return new ActionDisposable(() => Logger.LogInformation("<-- ({Name})", blockName));
    }

    /// <summary>
    /// Gets the analyzer test instance.
    /// </summary>
    /// <typeparam name="T">Type of analyzer.</typeparam>
    /// <returns>the analyzer test instance.</returns>
    protected static InjectableAnalyzerTest<T> GetAnalyzerTest<T>()
        where T : DiagnosticAnalyzer, new()
    {
        var ctx = new InjectableAnalyzerTest<T>
        {
            ReferenceAssemblies = ReferenceAssemblies.NetStandard.NetStandard20
        };
        ctx.TestState.AdditionalReferences.Add(typeof(CalmHandlerAttribute).Assembly);
        ctx.TestState.AdditionalReferences.Add(typeof(ValueTask).Assembly);
        ctx.TestState.Sources.Add(_testSource);
        return ctx;
    }

    /// <summary>
    /// The additional test source.
    /// </summary>
    private const string _testSource = /* lang=c#-test */ """
        using Calm.Core;

        public record TestCommand : ICalmCommand;
        public record TestCommandWithResponse : ICalmCommand<TestResponse>;
        public record TestQuery : ICalmQuery<TestResponse>;
        public record TestEvent : ICalmEvent;
        public record TestResponse;
        public record TestParameter;
        """;

    /// <summary>
    /// Gets a return value string.
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <returns>The return value string.</returns>
    /// <exception cref="ArgumentNullException">The returnType parameter is null.</exception>
    protected static string GetReturnValue(string returnType)
    {
        _ = returnType ?? throw new ArgumentNullException(nameof(returnType));

        return string.Equals(returnType, "Task", StringComparison.Ordinal)
            ? "Task.CompletedTask"
            : returnType.Replace("Task", "Task.FromResult") + "(new())";
    }
}
