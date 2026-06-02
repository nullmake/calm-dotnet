using Calm.Analyzers.Analyzers;
using Calm.Analyzers.Tests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Analyzers.Tests.Analyzers;

/// <summary>
/// Analyzer tests for CALM009 and CALM010.
/// </summary>
public class CALM009_010() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// CALM009: Invalid [CalmImmediate] usage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task InvalidCalmImmediateUsage()
    {
        var ctx = GetAnalyzerTest<CalmAttributeUsageAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;

            [{|CALM009:CalmImmediate|}]
            internal class NotAnEvent { }

            [CalmImmediate]
            internal class ValidEvent : ICalmEvent { }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// CALM010: Invalid [CalmSuppressLog] usage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task InvalidCalmSuppressLogUsage()
    {
        var ctx = GetAnalyzerTest<CalmAttributeUsageAnalyzer>();
        ctx.TestCode = /* lang=c#-test */ """
            using Calm.Core;

            [{|CALM010:CalmSuppressLog|}]
            internal class NotAMessage { }

            [CalmSuppressLog]
            internal class ValidCommand : ICalmCommand { }

            [CalmSuppressLog]
            internal class ValidQuery : ICalmQuery<string> { }

            [CalmSuppressLog]
            internal class ValidEvent : ICalmEvent { }
            """;
        await ctx.RunAsync(TestContext.Current.CancellationToken);
    }
}
