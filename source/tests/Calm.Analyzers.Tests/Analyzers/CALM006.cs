using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM006.
/// </summary>
public class CALM006() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM006: Invalid CancellationToken parameter.
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task", "TestCommand")]
    [InlineData("Task<TestResponse>", "TestCommandWithResponse")]
    [InlineData("Task<TestResponse>", "TestQuery")]
    [InlineData("Task", "TestEvent")]
    public async Task WrongTokenParameter(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} TestMethodAsync({{parameterType}} param, int {|CALM006:notAToken|})
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
