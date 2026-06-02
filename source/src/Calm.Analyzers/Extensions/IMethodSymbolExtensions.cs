using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Calm.Analyzers.Extensions;

/// <summary>
/// The extensions for <see cref="IMethodSymbol"/>.
/// </summary>
internal static class IMethodSymbolExtensions
{
    /// <summary>
    /// Determines whether the current method has the CalmHandlerAttribute.
    /// </summary>
    /// <param name="methodSymbol">The method or method-like symbol.</param>
    /// <returns>true if the current method has the CalmHandlerAttribute; otherwise, false.</returns>
    public static bool HasCalmHandlerAttribute(this IMethodSymbol methodSymbol)
        => methodSymbol.GetAttributes().Any(a => string.Equals(
            a.AttributeClass?.ToDisplayString(), "Calm.Core.CalmHandlerAttribute",
            StringComparison.Ordinal));

    /// <summary>
    /// Gets the location of return type.
    /// </summary>
    /// <param name="methodSymbol">The method or method-like symbol.</param>
    /// <returns>The location of return type.</returns>
    public static Location GetReturnTypeLocation(this IMethodSymbol methodSymbol)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference is not null)
        {
            var methodDeclaration = syntaxReference.GetSyntax(CancellationToken.None) as MethodDeclarationSyntax;
            if (methodDeclaration is not null)
            {
                return methodDeclaration.ReturnType.GetLocation();
            }
        }
        // If it cannot be retrieved, fall back to the method of location.
        return methodSymbol.Locations[0];
    }

    /// <summary>
    /// Determines whether the current method is a Calm async method (SendAsync, ExecuteAsync, etc.).
    /// </summary>
    /// <param name="methodSymbol">The method symbol.</param>
    /// <param name="compilation">The compilation.</param>
    /// <returns>true if it is a Calm async method; otherwise, false.</returns>
    public static bool IsCalmAsyncMethod(this IMethodSymbol methodSymbol, Compilation compilation)
    {
        var containingType = methodSymbol.ContainingType;
        if (containingType is null)
        {
            return false;
        }

        bool Implements(ITypeSymbol type, string interfaceName)
        {
            var interfaceType = compilation.GetTypeByMetadataName(interfaceName);
            if (interfaceType is null)
            {
                return false;
            }

            return SymbolEqualityComparer.Default.Equals(type, interfaceType)
                || type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType));
        }

        if (string.Equals(methodSymbol.Name, "SendAsync", StringComparison.Ordinal)
            || string.Equals(methodSymbol.Name, "PostAsync", StringComparison.Ordinal))
        {
            if (Implements(containingType, "Calm.Core.Messaging.Bus.ICalmCommandBus")
                || Implements(containingType, "Calm.Core.Messaging.Bus.ICalmQueryBus"))
            {
                return true;
            }
        }

        if (string.Equals(methodSymbol.Name, "PublishAsync", StringComparison.Ordinal))
        {
            if (Implements(containingType, "Calm.Core.Messaging.Bus.ICalmEventBus"))
            {
                return true;
            }
        }

        if (string.Equals(methodSymbol.Name, "ExecuteAsync", StringComparison.Ordinal)
            || string.Equals(methodSymbol.Name, "StopAsync", StringComparison.Ordinal)
            || string.Equals(methodSymbol.Name, "WaitForShutdownAsync", StringComparison.Ordinal)
            || string.Equals(methodSymbol.Name, "ScheduleAsync", StringComparison.Ordinal))
        {
            if (Implements(containingType, "Calm.Core.Engines.ICalmPump"))
            {
                return true;
            }
        }

        return false;
    }
}
