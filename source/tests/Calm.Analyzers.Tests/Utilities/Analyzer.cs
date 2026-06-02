using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Calm.Analyzers.Tests.Utilities;

/// <summary>
/// The analyzer class.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="Analyzer"/> class.
/// </remarks>
/// <param name="name">The analyzer name.</param>
/// <param name="path">The analyzer path.</param>
internal sealed class Analyzer(string name, string path)
{
    /// <summary>
    /// The analyzer name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The analyzer path.
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    /// The analyzer assembly.
    /// </summary>
    public Assembly Assembly => _assembly.Value;

    /// <summary>
    /// The analyzer assembly.
    /// </summary>
    [SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used",
        Justification = "To dynamically load a DLL.")]
    private readonly Lazy<Assembly> _assembly = new(() => Assembly.LoadFrom(path), true);

    /// <summary>
    /// Creates the `DiagnosticAnalyzer` instance.
    /// </summary>
    /// <param name="typeName">The type of `DiagnosticAnalyzer`</param>
    /// <returns>the `DiagnosticAnalyzer` instance.</returns>
    public DiagnosticAnalyzer CreateDiagnosticAnalyzer(string typeName)
    {
        var analyzerType = Assembly.GetType(typeName, true);
        return (DiagnosticAnalyzer)Activator.CreateInstance(analyzerType);
    }
}
