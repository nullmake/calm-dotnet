using Microsoft.CodeAnalysis;

namespace Calm.Analyzers.Extensions;

/// <summary>
/// The extensions for <see cref="Compilation"/>.
/// </summary>
internal static class CompilationExtensions
{
    /// <summary>
    /// Gets the type of the <see cref="CancellationToken"/>.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the <see cref="CancellationToken"/>; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfCancellationToken(this Compilation compilation)
        => compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

    /// <summary>
    /// Gets the type of the <see cref="Task"/>.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the <see cref="Task"/>; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfTask(this Compilation compilation)
        => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

    /// <summary>
    /// Gets the type of the <see cref="Task{T}"/>.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the <see cref="Task{T}"/>; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfTaskWithReturnValue(this Compilation compilation)
        => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

    /// <summary>
    /// Gets the type of the ICalmCommand.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the ICalmCommand; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfICalmCommand(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Calm.Core.ICalmCommand");

    /// <summary>
    /// Gets the type of the ICalmCommand&lt;T&gt;.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the ICalmCommand&lt;T&gt;; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfICalmCommandWithResponse(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Calm.Core.ICalmCommand`1");

    /// <summary>
    /// Gets the type of the ICalmQuery&lt;T&gt;.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the ICalmQuery&lt;T&gt;; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfICalmQuery(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Calm.Core.ICalmQuery`1");

    /// <summary>
    /// Gets the type of the ICalmEvent.
    /// </summary>
    /// <param name="compilation">The compilation object of the compiler.</param>
    /// <returns>The type of the ICalmEvent; null if the type can't be found.</returns>
    public static INamedTypeSymbol? GetTypeOfICalmEvent(this Compilation compilation)
        => compilation.GetTypeByMetadataName("Calm.Core.ICalmEvent");
}
