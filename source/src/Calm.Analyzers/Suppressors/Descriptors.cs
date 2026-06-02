using Microsoft.CodeAnalysis;

namespace Calm.Analyzers.Suppressors;

/// <summary>
/// The descriptors for suppressor.
/// </summary>
internal static class Descriptors
{
    /// <summary>
    /// CALMSP001: Suppression for unused Calm handlers.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP001 = new(
        "CALMSP001",
        "IDE0051",
        "Member is used as a Calm handler via reflection.");

    /// <summary>
    /// CALMSP002: Suppression for VSTHRD111 in Calm handlers.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP002 = new(
        "CALMSP002",
        "VSTHRD111",
        "Calm handlers are executed in a controlled context where ConfigureAwait is not required.");

    /// <summary>
    /// CALMSP003: Suppression for CA2007 in Calm handlers.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP003 = new(
        "CALMSP003",
        "CA2007",
        "Calm handlers are executed in a controlled context where ConfigureAwait is not required.");

    /// <summary>
    /// CALMSP004: Suppression for MA0004 in Calm handlers.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP004 = new(
        "CALMSP004",
        "MA0004",
        "Calm handlers are executed in a controlled context where ConfigureAwait is not required.");

    /// <summary>
    /// CALMSP005: Suppression for unused parameters in Calm handlers.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP005 = new(
        "CALMSP005",
        "IDE0060",
        "Calm handlers may have parameters that are not used by the implementation.");

    /// <summary>
    /// CALMSP006: Suppression for unused parameters in Calm handlers (Roslynator).
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP006 = new(
        "CALMSP006",
        "RCS1163",
        "Calm handlers may have parameters that are not used by the implementation.");

    /// <summary>
    /// CALMSP007: Suppression for VSTHRD111 in Calm methods.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP007 = new(
        "CALMSP007",
        "VSTHRD111",
        "Calm methods (SendAsync etc.) are executed in a controlled context where ConfigureAwait is not required.");

    /// <summary>
    /// CALMSP008: Suppression for CA2007 in Calm methods.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP008 = new(
        "CALMSP008",
        "CA2007",
        "Calm methods (SendAsync etc.) are executed in a controlled context where ConfigureAwait is not required.");

    /// <summary>
    /// CALMSP009: Suppression for MA0004 in Calm methods.
    /// </summary>
    public static readonly SuppressionDescriptor CALMSP009 = new(
        "CALMSP009",
        "MA0004",
        "Calm methods (SendAsync etc.) are executed in a controlled context where ConfigureAwait is not required.");
}
