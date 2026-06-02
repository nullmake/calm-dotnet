using Calm.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Calm.Analyzers.Suppressors;

/// <summary>
/// Suppresses "ConfigureAwait" related warnings for methods marked with [CalmHandler].
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CalmHandlerConfigureAwaitSuppressor : DiagnosticSuppressor
{
    /// <summary>
    /// The map of diagnostic IDs to suppression descriptors for inside Calm handlers.
    /// </summary>
    private static readonly ImmutableDictionary<string, SuppressionDescriptor> InsideHandlerSuppressionMap =
        new Dictionary<string, SuppressionDescriptor>(StringComparer.Ordinal)
        {
            { "VSTHRD111", Descriptors.CALMSP002 },
            { "CA2007", Descriptors.CALMSP003 },
            { "MA0004", Descriptors.CALMSP004 },
        }.ToImmutableDictionary();

    /// <summary>
    /// The map of diagnostic IDs to suppression descriptors for Calm methods.
    /// </summary>
    private static readonly ImmutableDictionary<string, SuppressionDescriptor> CalmMethodSuppressionMap =
        new Dictionary<string, SuppressionDescriptor>(StringComparer.Ordinal)
        {
            { "VSTHRD111", Descriptors.CALMSP007 },
            { "CA2007", Descriptors.CALMSP008 },
            { "MA0004", Descriptors.CALMSP009 },
        }.ToImmutableDictionary();

    /// <inheritdoc/>
    public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
        [.. InsideHandlerSuppressionMap.Values, .. CalmMethodSuppressionMap.Values];

    /// <inheritdoc/>
    public override void ReportSuppressions(SuppressionAnalysisContext context)
    {
        foreach (var diagnostic in context.ReportedDiagnostics)
        {
            var location = diagnostic.Location;
            var syntaxTree = location.SourceTree;
            if (syntaxTree is null)
            {
                continue;
            }

            var model = context.GetSemanticModel(syntaxTree);

            // 1. Check if it's inside a [CalmHandler] method
            if (InsideHandlerSuppressionMap.TryGetValue(diagnostic.Id, out var insideDescriptor))
            {
                var symbol = model.GetEnclosingSymbol(location.SourceSpan.Start, context.CancellationToken);
                var isInsideHandler = false;
                while (symbol is not null)
                {
                    if (symbol is IMethodSymbol methodSymbol && methodSymbol.HasCalmHandlerAttribute())
                    {
                        isInsideHandler = true;
                        context.ReportSuppression(Suppression.Create(insideDescriptor, diagnostic));
                        break;
                    }

                    symbol = symbol.ContainingSymbol;
                }

                if (isInsideHandler)
                {
                    continue;
                }
            }

            // 2. Check if it's a call to a Calm async method
            if (CalmMethodSuppressionMap.TryGetValue(diagnostic.Id, out var methodDescriptor))
            {
                var root = syntaxTree.GetRoot(context.CancellationToken);
                var node = root.FindNode(location.SourceSpan);

                // CA2007/VSTHRD111 are usually on the expression being awaited.
                var expression = node;
                if (node is AwaitExpressionSyntax awaitExpression)
                {
                    expression = awaitExpression.Expression;
                }

                var operation = model.GetOperation(expression, context.CancellationToken);
                if (operation is IInvocationOperation invocationOperation)
                {
                    if (invocationOperation.TargetMethod.IsCalmAsyncMethod(context.Compilation))
                    {
                        context.ReportSuppression(Suppression.Create(methodDescriptor, diagnostic));
                    }
                }
            }
        }
    }
}
