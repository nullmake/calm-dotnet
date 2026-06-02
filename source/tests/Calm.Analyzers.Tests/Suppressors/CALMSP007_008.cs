using Calm.Analyzers.Suppressors;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Suppressors;

/// <summary>
/// Suppressor tests for CALMSP007 and CALMSP008.
/// </summary>
public class CALMSP007_008() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CA2007: Should be suppressed for Calm methods.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CalmMethod_CA2007_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.CA2007);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using Calm.Core.Messaging.Bus;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                public async Task MethodAsync(ICalmCommandBus bus, TestCommand command)
                {
                    await {|#0:bus.SendAsync(command)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// VSTHRD111: Should be suppressed for Calm methods.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CalmMethod_VSTHRD111_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.VSTHRD111);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using Calm.Core.Messaging.Bus;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                public async Task MethodAsync(ICalmCommandBus bus, TestCommand command)
                {
                    await {|#0:bus.SendAsync(command)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("VSTHRD111", DiagnosticSeverity.Hidden)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CA2007: Should be suppressed for PublishAsync, ScheduleAsync, and PostAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task NewMethods_CA2007_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.CA2007);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using Calm.Core.Messaging.Bus;
            using Calm.Core.Engines;
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                public async Task MethodAsync(ICalmCommandBus commandBus, ICalmEventBus eventBus, ICalmPump pump, TestCommand command, TestEvent @event)
                {
                    await {|#0:commandBus.PostAsync(command)|};
                    await {|#1:eventBus.PublishAsync(@event)|};
                    await {|#2:pump.ScheduleAsync(ct => Task.CompletedTask)|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(1, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(2, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CA2007: Should be suppressed for CalmEngine class methods.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CalmEngineClass_CA2007_Suppressed()
    {
        var ctx = GetAnalyzerTest<CalmHandlerConfigureAwaitSuppressor>();
        ctx.AdditionalAnalyzers.Add(DiagnosticAnalyzers.CA2007);
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            
            internal class TestClass
            {
                public async Task MethodAsync(CalmEngine engine)
                {
                    await {|#0:engine.ExecuteAsync(ct => Task.CompletedTask)|};
                    await {|#1:engine.ScheduleAsync(ct => Task.CompletedTask)|};
                    await {|#2:engine.StopAsync()|};
                }
            }
            """;
        ctx.ExpectedDiagnostics.AddRange(
            [
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(0, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(1, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true),
                new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
                    .WithLocation(2, DiagnosticLocationOptions.UnnecessaryCode)
                    .WithIsSuppressed(true)
            ]);
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
