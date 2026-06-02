using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM005.
/// </summary>
public class CALM005() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM005: Response type mismatch (expected Task of TestResponse for TestCommandWithResponse).
    /// </summary>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("TestCommandWithResponse")]
    [InlineData("TestQuery")]
    public async Task WrongResponseType(string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {|CALM005:Task<int>|} TestMethodAsync({{parameterType}} param, CancellationToken token)
                    => Task.FromResult(0);
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
