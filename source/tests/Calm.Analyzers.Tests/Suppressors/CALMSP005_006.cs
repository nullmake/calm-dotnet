using Calm.Analyzers.Suppressors;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Suppressors;

/// <summary>
/// Suppressor tests for CALMSP005.
/// </summary>
public class CALMSP005_006() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Unused parameter: Should not trigger IDE0060 (handled by Suppressor).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UnusedParameter_IDE0060_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerUnusedSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.IDE0060);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public Task HandleAsync(TestCommand {|#0:param|}, CancellationToken {|#1:token|})
                    => Task.CompletedTask;
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("IDE0060", DiagnosticSeverity.Info)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("IDE0060", DiagnosticSeverity.Info)
                    .WithLocation(1, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Unused parameter: Should not trigger RCS1163 (handled by Suppressor).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UnusedParameter_RCS1163_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerUnusedSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.RCS1163);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public Task HandleAsync({|#0:TestCommand param|}, {|#1:CancellationToken token|})
                    => Task.CompletedTask;
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("RCS1163", DiagnosticSeverity.Info)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("RCS1163", DiagnosticSeverity.Info)
                    .WithLocation(1, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
