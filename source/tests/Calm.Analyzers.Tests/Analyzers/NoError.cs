using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for no error.
/// </summary>
public class NoError() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// This should NOT trigger any error (valid signature).
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task", "TestCommand")]
    [InlineData("Task<TestResponse>", "TestCommandWithResponse")]
    [InlineData("Task<TestResponse>", "TestQuery")]
    [InlineData("Task", "TestEvent")]
    public async Task ValidHandler(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} TestMethodAsync({{parameterType}} param, CancellationToken token)
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
