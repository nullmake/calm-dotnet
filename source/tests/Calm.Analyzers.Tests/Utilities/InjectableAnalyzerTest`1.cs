using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Analyzers.Tests.Utilities;

/// <summary>
/// The analyzer test class.
/// </summary>
/// <typeparam name="T">Type of analyzer.</typeparam>
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal",
    Justification = "Test classes and related classes must be public.")]
public sealed class InjectableAnalyzerTest<T> : CSharpAnalyzerTest<T, DefaultVerifier>
    where T : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Additional `DiagnosticAnalyzers`.
    /// </summary>
    public ICollection<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = [];

    /// <inheritdoc/>
    protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
    {
        foreach (var analyzer in AdditionalAnalyzers)
        {
            yield return analyzer;
        }

        foreach (var analyzer in base.GetDiagnosticAnalyzers())
        {
            yield return analyzer;
        }
    }
}
