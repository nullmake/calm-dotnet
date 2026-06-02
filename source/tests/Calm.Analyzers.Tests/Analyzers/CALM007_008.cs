using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM007 and CALM008.
/// </summary>
public class CALM007_008() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM007: ConfigureAwait(false) on Calm method should trigger warning inside CalmHandler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CALM007_TriggeredInsideHandler()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using Calm.Core.Messaging.Bus;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public async Task HandleAsync(TestCommand param, ICalmCommandBus bus, CancellationToken token)
                {
                    await {|#0:bus.SendAsync(param).ConfigureAwait(false)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.Add(
            new DiagnosticResult("CALM007", DiagnosticSeverity.Warning)
                .WithLocation(0));
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM007/008: Should not trigger outside CalmHandler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CALM_NoDiagnosticOutsideHandler()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using Calm.Core.Messaging.Bus;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                public async Task MethodAsync(ICalmCommandBus bus, TestCommand command)
                {
                    await bus.SendAsync(command).ConfigureAwait(false);
                    await Task.Delay(1).ConfigureAwait(false);
                }
            }
            """;
        // No diagnostics expected
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM008: ConfigureAwait(false) inside CalmHandler should trigger warning.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CALM008_Triggered()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                [CalmHandler]
                public async Task HandleAsync(TestCommand param, CancellationToken token)
                {
                    await {|#0:Task.Delay(1).ConfigureAwait(false)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.Add(
            new DiagnosticResult("CALM008", DiagnosticSeverity.Warning)
                .WithLocation(0));
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
