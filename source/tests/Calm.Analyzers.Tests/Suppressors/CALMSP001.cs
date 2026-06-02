using Calm.Analyzers.Suppressors;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Suppressors;

/// <summary>
/// Suppressor tests for CALMSP001.
/// </summary>
public class CALMSP001() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Private handler: Should not trigger IDE0051 (handled by Suppressor).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <param name="returnType">The name of the return type.</param>
    /// <param name="parameterType">The name of the parameter type.</param>
    [Theory]
    [InlineData("Task", "TestCommand")]
    [InlineData("Task<TestResponse>", "TestCommandWithResponse")]
    [InlineData("Task<TestResponse>", "TestQuery")]
    [InlineData("Task", "TestEvent")]
    public async Task PrivateHandler(string returnType, string parameterType)
    {
        var ctx = GetAnalyzerTest<CalmHandlerUnusedSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.IDE0051);
        ctx.TestCode = /* lang=c#-test */ $$"""
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                {|#1:[CalmHandler]
                private {{returnType}} {|#0:TestMethodAsync|}({{parameterType}} param, CancellationToken token)
                    => {{GetReturnValue(returnType)}};|}
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("IDE0051", DiagnosticSeverity.Info)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithLocation(1, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
