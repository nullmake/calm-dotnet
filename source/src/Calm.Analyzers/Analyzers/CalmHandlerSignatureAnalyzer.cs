using Calm.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Calm.Analyzers.Analyzers;

/// <summary>
/// Analyzes methods marked with [CalmHandler] to ensure they have the correct signature.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CalmHandlerSignatureAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [
            Descriptors.CALM001,
            Descriptors.CALM002,
            Descriptors.CALM003,
            Descriptors.CALM004,
            Descriptors.CALM005,
            Descriptors.CALM006
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
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    /// <summary>
    /// Analyzes a method symbol for the [CalmHandler] attribute.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol methodSymbol
            || !methodSymbol.HasCalmHandlerAttribute())
        {
            return;
        }

        if (methodSymbol.Parameters.Length is not 2)
        {
            context.ReportDiagnostic(methodSymbol, Descriptors.CALM001);
            return;
        }

        var tokenParam = methodSymbol.Parameters[1];
        if (!tokenParam.Type.IsCancellationToken(context.Compilation))
        {
            context.ReportDiagnostic(tokenParam, Descriptors.CALM006);
        }

        var messageParam = methodSymbol.Parameters[0];
        var messageType = messageParam.Type;
        var compilation = context.Compilation;

        if (messageType.TryGetImplementedInterface(compilation.GetTypeOfICalmCommand(), out _))
        {
            ValidateReturnType(context, methodSymbol);
        }
        else if (messageType.TryGetImplementedInterface(compilation.GetTypeOfICalmEvent(), out _))
        {
            ValidateReturnType(context, methodSymbol);
        }
        else if (messageType.TryGetImplementedInterface(
            compilation.GetTypeOfICalmCommandWithResponse(), out var typeOfCommandWithResponse))
        {
            ValidateReturnTypeWithResponse(context, methodSymbol, typeOfCommandWithResponse);
        }
        else if (messageType.TryGetImplementedInterface(
            compilation.GetTypeOfICalmQuery(), out var typeOfQuery))
        {
            ValidateReturnTypeWithResponse(context, methodSymbol, typeOfQuery);
        }
        else
        {
            context.ReportDiagnostic(messageParam, Descriptors.CALM004);
        }
    }

    /// <summary>
    /// Validates the return type for handlers that do not expect a response.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="methodSymbol">The method symbol to validate.</param>
    private static void ValidateReturnType(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.ReturnType.IsTask(context.Compilation))
        {
            context.ReportDiagnostic(methodSymbol.GetReturnTypeLocation(), Descriptors.CALM002);
        }
    }

    /// <summary>
    /// Validates the return type for handlers that expect a response.
    /// </summary>
    /// <param name="context">The symbol analysis context.</param>
    /// <param name="methodSymbol">The method symbol to validate.</param>
    /// <param name="typeOfRequest">The implemented request interface (Command or Query).</param>
    private static void ValidateReturnTypeWithResponse(SymbolAnalysisContext context,
        IMethodSymbol methodSymbol, INamedTypeSymbol typeOfRequest)
    {
        var returnType = methodSymbol.ReturnType;
        if (!returnType.IsTaskWithReturnValue(context.Compilation))
        {
            context.ReportDiagnostic(methodSymbol.GetReturnTypeLocation(), Descriptors.CALM003);
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(
            typeOfRequest.TypeArguments[0],
            (returnType as INamedTypeSymbol)?.TypeArguments[0]))
        {
            context.ReportDiagnostic(methodSymbol.GetReturnTypeLocation(), Descriptors.CALM005);
        }
    }
}
