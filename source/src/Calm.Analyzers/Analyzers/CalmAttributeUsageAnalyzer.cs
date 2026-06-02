using Calm.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Calm.Analyzers.Analyzers;

/// <summary>
/// Analyzer for validating the usage of CALM attributes on message types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CalmAttributeUsageAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            Descriptors.CALM009,
            Descriptors.CALM010
        ];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    /// <summary>
    /// Analyzes a type symbol for the CALM attributes.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        var immediateAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(
                a.AttributeClass?.ToDisplayString(),
                "Calm.Core.CalmImmediateAttribute",
                StringComparison.Ordinal));

        if (immediateAttr is not null)
        {
            var eventType = context.Compilation.GetTypeByMetadataName("Calm.Core.ICalmEvent");
            if (!typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, eventType)))
            {
                var location = immediateAttr.ApplicationSyntaxReference?
                    .GetSyntax(context.CancellationToken).GetLocation() ?? typeSymbol.Locations[0];
                context.ReportDiagnostic(location, Descriptors.CALM009);
            }
        }

        var suppressLogAttr = typeSymbol.GetAttributes()
            .FirstOrDefault(a => string.Equals(
                a.AttributeClass?.ToDisplayString(),
                "Calm.Core.CalmSuppressLogAttribute",
                StringComparison.Ordinal));

        if (suppressLogAttr is not null)
        {
            var messageType = context.Compilation.GetTypeByMetadataName("Calm.Core.Messaging.ICalmMessage");
            var requestType = context.Compilation.GetTypeByMetadataName("Calm.Core.Messaging.ICalmRequest`1");

            var implementsMessage = typeSymbol.AllInterfaces
                .Any(i => SymbolEqualityComparer.Default.Equals(i, messageType));
            var implementsRequest = typeSymbol.AllInterfaces
                .Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, requestType));

            if (!implementsMessage && !implementsRequest)
            {
                var location = suppressLogAttr.ApplicationSyntaxReference?
                    .GetSyntax(context.CancellationToken).GetLocation() ?? typeSymbol.Locations[0];
                context.ReportDiagnostic(location, Descriptors.CALM010);
            }
        }
    }
}
