#if !NETCOREAPP3_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates that a parameter captures the expression passed for another parameter as a string.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute"/> class.
/// </remarks>
/// <param name="parameterName">The name of the parameter whose expression should be captured as a string.</param>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
#pragma warning disable MA0182 // Avoid unused internal types
internal sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
#pragma warning restore MA0182 // Avoid unused internal types
{
    /// <summary>
    /// The name of the parameter whose expression should be captured as a string.
    /// </summary>
    public string ParameterName { get; } = parameterName;
}
#endif
