using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Analyzers.Extensions;

/// <summary>
/// The extensions for <see cref="ITypeSymbol"/>.
/// </summary>
internal static class ITypeSymbolExtensions
{
    /// <summary>
    /// Gets an interface implemented by a type.
    /// </summary>
    /// <param name="typeSymbol">The current symbol.</param>
    /// <param name="interfaceSymbol">The generic interface symbol to look for.</param>
    /// <param name="implemented">The implemented interface.</param>
    /// <returns>true if got the implemented interface; otherwise, false.</returns>
    public static bool TryGetImplementedInterface(this ITypeSymbol typeSymbol, INamedTypeSymbol? interfaceSymbol,
        [MaybeNullWhen(false)] out INamedTypeSymbol implemented)
    {
        implemented = typeSymbol.AllInterfaces
            .FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol));
        return implemented is not null;
    }

    /// <summary>
    /// Determines whether the current symbol is the <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="symbol">The current symbol.</param>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>true if the current symbol is the <see cref="CancellationToken"/>;
    /// otherwise, false.</returns>
    public static bool IsCancellationToken(this ITypeSymbol symbol, Compilation compilation)
        => SymbolEqualityComparer.Default.Equals(
            symbol.OriginalDefinition,
            compilation.GetTypeOfCancellationToken());

    /// <summary>
    /// Determines whether the current symbol is the <see cref="Task"/>.
    /// </summary>
    /// <param name="symbol">The current symbol.</param>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>true if the current symbol is the <see cref="Task"/>;
    /// otherwise, false.</returns>
    public static bool IsTask(this ITypeSymbol symbol, Compilation compilation)
        => SymbolEqualityComparer.Default.Equals(
            symbol.OriginalDefinition,
            compilation.GetTypeOfTask());

    /// <summary>
    /// Determines whether the current symbol is the <see cref="Task{T}"/>.
    /// </summary>
    /// <param name="symbol">The current symbol.</param>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>true if the current symbol is the <see cref="Task{T}"/>;
    /// otherwise, false.</returns>
    public static bool IsTaskWithReturnValue(this ITypeSymbol symbol, Compilation compilation)
        => SymbolEqualityComparer.Default.Equals(
            symbol.OriginalDefinition,
            compilation.GetTypeOfTaskWithReturnValue());
}
