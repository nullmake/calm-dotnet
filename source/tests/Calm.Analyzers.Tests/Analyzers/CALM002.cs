using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM002.
/// </summary>
public class CALM002() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM002: Invalid return type (void).
    /// </summary>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("TestCommand")]
    [InlineData("TestEvent")]
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
                public {|CALM002:void|} TestMethodAsync({{parameterType}} param, CancellationToken token)
                {
                    // do nothing;
                }
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM002: Invalid return type (ValueTask).
    /// </summary>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("TestCommand")]
    [InlineData("TestEvent")]
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
                public {|CALM002:ValueTask|} TestMethodAsync({{parameterType}} param, CancellationToken token)
                    => new();
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
