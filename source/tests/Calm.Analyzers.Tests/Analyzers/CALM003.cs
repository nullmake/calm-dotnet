using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM003.
/// </summary>
public class CALM003() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM003: Invalid return type (void).
    /// </summary>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("TestCommandWithResponse")]
    [InlineData("TestQuery")]
    public async Task VoidReturnType(string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {|CALM003:void|} TestMethodAsync({{parameterType}} param, CancellationToken token)
                {
                    // do nothing;
                }
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM003: Invalid return type (ValueTask).
    /// </summary>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("TestCommandWithResponse")]
    [InlineData("TestQuery")]
    public async Task ValueTaskReturnType(string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {|CALM003:ValueTask|} TestMethodAsync({{parameterType}} param, CancellationToken token)
                    => new();
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
