using Calm.Analyzers.Suppressors;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Suppressors;

/// <summary>
/// Suppressor tests for CALMSP002, CALMSP003, and CALMSP004.
/// </summary>
public class CALMSP002_004() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// VSTHRD111: Should be suppressed in [CalmHandler].
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task VSTHRD111_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.VSTHRD111);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public async Task HandleAsync(TestCommand param, CancellationToken token)
                {
                    await {|#0:Task.Delay(1)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.Add(
            new DiagnosticResult("VSTHRD111", DiagnosticSeverity.Hidden)
                .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                .WithIsSuppressed(true));
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CA2007: Should be suppressed in [CalmHandler].
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CA2007_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.CA2007);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public async Task HandleAsync(TestCommand param, CancellationToken token)
                {
                    await {|#0:Task.Delay(1)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.Add(
            new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                .WithIsSuppressed(true));
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
