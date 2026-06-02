using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM001.
/// </summary>
public class CALM001() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM001: Invalid argument count.
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task", "TestCommand")]
    [InlineData("Task<TestResponse>", "TestCommandWithResponse")]
    [InlineData("Task<TestResponse>", "TestQuery")]
    [InlineData("Task", "TestEvent")]
    public async Task TooManyArguments(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} {|CALM001:TestMethodAsync|}({{parameterType}} param, CancellationToken token, int extra)
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM001: Invalid argument count (missing CancellationToken).
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task", "TestCommand")]
    [InlineData("Task<TestResponse>", "TestCommandWithResponse")]
    [InlineData("Task<TestResponse>", "TestQuery")]
    [InlineData("Task", "TestEvent")]
    public async Task MissingCancellationToken(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} {|CALM001:TestMethodAsync|}({{parameterType}} param)
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
