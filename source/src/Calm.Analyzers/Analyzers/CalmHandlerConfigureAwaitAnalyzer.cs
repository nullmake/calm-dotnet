using Calm.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace Calm.Analyzers.Analyzers;

/// <summary>
/// Analyzer for ConfigureAwait usage in Calm context.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CalmHandlerConfigureAwaitAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            Descriptors.CALM007,
            Descriptors.CALM008,
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
        context.RegisterOperationAction(AnalyzeAwait, OperationKind.Await);
    }

    /// <summary>
    /// Analyzes the await operation.
    /// </summary>
    /// <param name="context">The operation analysis context.</param>
    private static void AnalyzeAwait(OperationAnalysisContext context)
    {
        var awaitOperation = (IAwaitOperation)context.Operation;
        var operand = awaitOperation.Operation;

        // Check if it's task.ConfigureAwait(bool)
        if (operand is IInvocationOperation invocation
            && string.Equals(invocation.TargetMethod.Name, "ConfigureAwait", StringComparison.Ordinal)
            && invocation.Arguments.Length is 1)
        {
            // First, check if we are inside a [CalmHandler]
            var symbol = context.ContainingSymbol;
            var isInsideHandler = false;
            while (symbol is not null)
            {
                if (symbol is IMethodSymbol methodSymbol && methodSymbol.HasCalmHandlerAttribute())
                {
                    isInsideHandler = true;
                    break;
                }
                symbol = symbol.ContainingSymbol;
            }

            // We only enforce "No ConfigureAwait" policy inside Calm handlers.
            if (!isInsideHandler)
            {
                return;
            }

            // It is ConfigureAwait(false)
            var argument = invocation.Arguments[0].Value;
            if (argument is ILiteralOperation literal
                && literal.ConstantValue.HasValue && literal.ConstantValue.Value is false)
            {
                // Check if the original task was from a Calm method
                var originalTask = invocation.Instance;
                if (originalTask is IInvocationOperation taskInvocation
                    && taskInvocation.TargetMethod.IsCalmAsyncMethod(context.Compilation))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.CALM007, invocation.Syntax.GetLocation()));
                    return;
                }

                // If we reach here, we are inside a [CalmHandler] but it's not a Calm method.
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.CALM008, invocation.Syntax.GetLocation()));
            }
        }
    }
}
