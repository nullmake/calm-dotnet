using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM004.
/// </summary>
public class CALM004() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM004: Message type mismatch (int is not ICalmMessage).
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task")]
    [InlineData("Task<TestResponse>")]
    public async Task NotAMessage(string returnType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} TestMethodAsync(int {|CALM004:param|}, CancellationToken token)
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM004: Message type mismatch.
    /// </summary>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData("Task", "ICalmCommand")]
    [InlineData("Task<TestResponse>", "ICalmCommand<TestResponse>")]
    [InlineData("Task<TestResponse>", "ICalmQuery<TestResponse>")]
    [InlineData("Task", "ICalmEvent")]
    [InlineData("Task<TestResponse>", "TestParameter")]
    [InlineData("Task", "TestParameter")]
    public async Task WrongParameterType(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerSignatureAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;

            internal class TestClass
            {
                [CalmHandler]
                public {{returnType}} TestMethodAsync({{parameterType}} {|CALM004:param|}, CancellationToken token)
                    => {{GetReturnValue(returnType)}};
            }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
