using Microsoft.CodeAnalysis;

namespace Calm.Analyzers.Analyzers;

/// <summary>
/// The descriptors for analyzer.
/// </summary>
internal static class Descriptors
{
    /// <summary>
    /// CALM001: Invalid argument count.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM001 = new(
        nameof(CALM001),
        "Invalid CalmHandler signature: Invalid argument count",
        "The [CalmHandler] method must have exactly two parameters: (TMessage, CancellationToken)",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM002: Invalid return type.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM002 = new(
        nameof(CALM002),
        "Invalid CalmHandler signature: Invalid return type",
        "The [CalmHandler] method must return Task",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM003: Invalid return type.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM003 = new(
        nameof(CALM003),
        "Invalid CalmHandler signature: Invalid return type",
        "The [CalmHandler] method must return Task<TResponse>",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM004: Message type mismatch.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM004 = new(
        nameof(CALM004),
        "Invalid CalmHandler signature: Message type mismatch",
        "The first parameter of the [CalmHandler] method must be a type derived from ICalmCommand, ICalmQuery, or ICalmEvent",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM005: Response type mismatch.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM005 = new(
        nameof(CALM005),
        "Invalid CalmHandler signature: Response type mismatch",
        "The return type Task<T> must match the TResponse defined in the message interface",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM006: Invalid CancellationToken.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM006 = new(
        nameof(CALM006),
        "Invalid CalmHandler signature: Invalid CancellationToken",
        "The second parameter of the [CalmHandler] method must be System.Threading.CancellationToken",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM007: Do not use ConfigureAwait(false) on Calm methods.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM007 = new(
        nameof(CALM007),
        "Do not use ConfigureAwait(false) on Calm methods",
        "Calm methods (SendAsync etc.) should not use ConfigureAwait(false) as they rely on the engine context",
        nameof(Category.Usage),
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM008: Do not use ConfigureAwait(false) inside Calm handlers.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM008 = new(
        nameof(CALM008),
        "Do not use ConfigureAwait(false) inside Calm handlers",
        "Methods marked with [CalmHandler] should not use ConfigureAwait(false) to ensure they stay on the engine context",
        nameof(Category.Usage),
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM009: Invalid [CalmImmediate] usage.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM009 = new(
        nameof(CALM009),
        "Invalid [CalmImmediate] usage",
        "The [CalmImmediate] attribute can only be applied to types that implement ICalmEvent",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// CALM010: Invalid [CalmSuppressLog] usage.
    /// </summary>
    public static readonly DiagnosticDescriptor CALM010 = new(
        nameof(CALM010),
        "Invalid [CalmSuppressLog] usage",
        "The [CalmSuppressLog] attribute can only be applied to types that implement ICalmMessage or ICalmRequest<TResponse>",
        nameof(Category.Usage),
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
