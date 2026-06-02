using Microsoft.CodeAnalysis;

namespace Calm.Analyzers.Analyzers;

/// <summary>
/// The category of the <see cref="DiagnosticDescriptor"/>.
/// </summary>
/// <remarks>https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/categories</remarks>
internal enum Category
{
    /// <summary>
    /// Usage rules.
    /// </summary>
    Usage,
}
