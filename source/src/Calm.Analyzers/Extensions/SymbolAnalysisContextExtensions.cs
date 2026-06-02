using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Calm.Analyzers.Extensions;

/// <summary>
/// The extensions for <see cref="SymbolAnalysisContext"/>.
/// </summary>
internal static class SymbolAnalysisContextExtensions
{
    /// <summary>
    /// Create a diagnostic for the symbol.
    /// </summary>
    /// <param name="context">Context for a symbol action.</param>
    /// <param name="symbol">The diagnostic target symbol.</param>
    /// <param name="descriptor">The instance of a <see cref="DiagnosticDescriptor"/>.</param>
    public static void ReportDiagnostic(this SymbolAnalysisContext context,
        ISymbol symbol, DiagnosticDescriptor descriptor)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, symbol.Locations[0]));

    /// <summary>
    /// Create a diagnostic for the location.
    /// </summary>
    /// <param name="context">Context for a symbol action.</param>
    /// <param name="location">The diagnostic target location.</param>
    /// <param name="descriptor">The instance of a <see cref="DiagnosticDescriptor"/>.</param>
    public static void ReportDiagnostic(this SymbolAnalysisContext context,
        Location location, DiagnosticDescriptor descriptor)
        => context.ReportDiagnostic(Diagnostic.Create(descriptor, location));
}
