using Calm.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Calm.Analyzers.Suppressors;

/// <summary>
/// Suppresses "unused member" warnings for methods marked with [CalmHandler].
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CalmHandlerUnusedSuppressor : DiagnosticSuppressor
{
    /// <summary>
    /// The map of diagnostic IDs to suppression descriptors.
    /// </summary>
    private static readonly ImmutableDictionary<string, SuppressionDescriptor> SuppressionMap =
        new Dictionary<string, SuppressionDescriptor>(StringComparer.Ordinal)
        {
            { "IDE0051", Descriptors.CALMSP001 },
            { "IDE0060", Descriptors.CALMSP005 },
            { "RCS1163", Descriptors.CALMSP006 },
        }.ToImmutableDictionary();

    /// <inheritdoc/>
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions => [.. SuppressionMap.Values];

    /// <inheritdoc/>
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            if (!SuppressionMap.TryGetValue(diagnostic.Id, out var descriptor))
            {
                continue;
            }

            var location = diagnostic.Location;
            var syntaxTree = location.SourceTree;
            if (syntaxTree is null)
            {
                continue;
            }

            var model = context.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot(context.CancellationToken);
            var node = root.FindNode(location.SourceSpan);

            var symbol = model.GetDeclaredSymbol(node, context.CancellationToken);
            if (symbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.HasCalmHandlerAttribute())
                {
                    context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
                }
            }
            else if (symbol is IParameterSymbol parameterSymbol)
            {
                if (parameterSymbol.ContainingSymbol is IMethodSymbol containingMethod
                    && containingMethod.HasCalmHandlerAttribute())
                {
                    context.ReportSuppression(Suppression.Create(descriptor, diagnostic));
                }
            }
            else
            {
                // do nothing
            }
        }
    }
}
