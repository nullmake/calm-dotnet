using System.Reflection;

namespace Calm.Analyzers.Tests.Utilities;

/// <summary>
/// The PackageReference information.
/// </summary>
internal static class PackageReferences
{
    /// <summary>
    /// The path of the `Microsoft.CodeAnalysis.CSharp.NetAnalyzers`.
    /// </summary>
    public static Analyzer MicrosoftCodeAnalysisCSharpNetAnalyzers { get; }

    /// <summary>
    /// The path of the `Microsoft.CodeAnalysis.NetAnalyzers`.
    /// </summary>
    public static Analyzer MicrosoftCodeAnalysisNetAnalyzers { get; }

    /// <summary>
    /// The path of the `Microsoft.CodeAnalysis.CSharp.Features`.
    /// </summary>
    public static Analyzer MicrosoftCodeAnalysisCSharpFeatures { get; }

    /// <summary>
    /// The path of the `Meziantou.Analyzer`.
    /// </summary>
    public static Analyzer MeziantouAnalyzer { get; }

    /// <summary>
    /// The path of the `Microsoft.VisualStudio.Threading.Analyzers`.
    /// </summary>
    public static Analyzer MicrosoftVisualStudioThreadingAnalyzers { get; }

    /// <summary>
    /// The path of the `Roslynator.Analyzers`.
    /// </summary>
    public static Analyzer RoslynatorAnalyzers { get; }

    /// <summary>
    /// The path of the `SonarAnalyzer.CSharp`.
    /// </summary>
    public static Analyzer SonarAnalyzerCSharp { get; }

    /// <summary>
    /// Initializes the <see cref="PackageReferences"/> class.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    static PackageReferences()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        var assembly = typeof(PackageReferences).Assembly;
        foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            var keys = attr.Key.Split('_');
            if (keys is null || keys.Length is not 3 || keys[0] is null || keys[1] is null
                || !File.Exists(attr.Value))
            {
                continue;
            }

            var category = keys[0];
            var name = keys[1];
            var path = attr.Value;
            if (string.Equals(category, "Analyzer", StringComparison.Ordinal))
            {
                switch (name)
                {
                    case "Microsoft.CodeAnalysis.CSharp.NetAnalyzers":
                        MicrosoftCodeAnalysisCSharpNetAnalyzers = new Analyzer(name, path);
                        break;
                    case "Microsoft.CodeAnalysis.NetAnalyzers":
                        MicrosoftCodeAnalysisNetAnalyzers = new Analyzer(name, path);
                        break;
                    case "Meziantou.Analyzer":
                        MeziantouAnalyzer = new Analyzer(name, path);
                        break;
                    case "Microsoft.VisualStudio.Threading.Analyzers":
                        MicrosoftVisualStudioThreadingAnalyzers = new Analyzer(name, path);
                        break;
                    case "Roslynator.CSharp.Analyzers":
                        RoslynatorAnalyzers = new Analyzer(name, path);
                        break;
                    case "SonarAnalyzer.CSharp":
                        SonarAnalyzerCSharp = new Analyzer(name, path);
                        break;
                    default:
                        break;
                }
            }

            if (string.Equals(category, "Reference", StringComparison.Ordinal))
            {
#pragma warning disable S1301 // "switch" statements should have at least 3 "case" clauses
                switch (name)
                {
                    case "Microsoft.CodeAnalysis.CSharp.Features":
                        MicrosoftCodeAnalysisCSharpFeatures = new Analyzer(name, path);
                        break;
                    default:
                        break;
                }
#pragma warning restore S1301 // "switch" statements should have at least 3 "case" clauses
            }
        }
    }
}
