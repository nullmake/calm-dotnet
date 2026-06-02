using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Analyzers.Tests.Utilities;

/// <summary>
/// IDEXXXX series diagnostic analyzers.
/// </summary>
[SuppressMessage("Minor Code Smell", "S100:Methods and properties should be named in PascalCase",
    Justification = "Do not use PascalCase for rule names.")]
internal static class DiagnosticAnalyzers
{
    /// <summary>
    /// Gets the CA2007 `DiagnosticAnalyzer`.
    /// </summary>
    public static DiagnosticAnalyzer CA2007 => PackageReferences.MicrosoftCodeAnalysisNetAnalyzers
        .CreateDiagnosticAnalyzer(
            "Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer");

    /// <summary>
    /// Gets the IDE0051 `DiagnosticAnalyzer`.
    /// </summary>
    public static DiagnosticAnalyzer IDE0051 => PackageReferences.MicrosoftCodeAnalysisCSharpFeatures
        .CreateDiagnosticAnalyzer(
            "Microsoft.CodeAnalysis.CSharp.RemoveUnusedMembers.CSharpRemoveUnusedMembersDiagnosticAnalyzer");

    /// <summary>
    /// Gets the IDE0060 `DiagnosticAnalyzer`.
    /// </summary>
    public static DiagnosticAnalyzer IDE0060 => PackageReferences.MicrosoftCodeAnalysisCSharpFeatures
        .CreateDiagnosticAnalyzer(
            "Microsoft.CodeAnalysis.CSharp.RemoveUnusedParametersAndValues.CSharpRemoveUnusedParametersAndValuesDiagnosticAnalyzer");

    /// <summary>
    /// Gets the VSTHRD111 `DiagnosticAnalyzer`.
    /// </summary>
    public static DiagnosticAnalyzer VSTHRD111 => PackageReferences.MicrosoftVisualStudioThreadingAnalyzers
        .CreateDiagnosticAnalyzer(
            "Microsoft.VisualStudio.Threading.Analyzers.VSTHRD111UseConfigureAwaitAnalyzer");

    /// <summary>
    /// Gets the RCS1163 `DiagnosticAnalyzer`.
    /// </summary>
    public static DiagnosticAnalyzer RCS1163 => PackageReferences.RoslynatorAnalyzers
        .CreateDiagnosticAnalyzer(
            "Roslynator.CSharp.Analysis.UnusedParameter.UnusedParameterAnalyzer");
}
