#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that when a method returns <see cref="ReturnValue"/>,
/// the parameter may be null even if the corresponding type disallows it.
/// </summary>
/// <remarks>
/// Initializes the attribute with the specified return value condition.
/// </remarks>
/// <param name="returnValue">The return value condition.
/// If the method returns this value, the associated parameter may be null.</param>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
#pragma warning disable MA0182 // Avoid unused internal types
internal sealed class MaybeNullWhenAttribute(bool returnValue) : Attribute
#pragma warning restore MA0182 // Avoid unused internal types
{
    /// <summary>
    /// Gets the return value condition.
    /// </summary>
    public bool ReturnValue { get; } = returnValue;
}
#endif
