#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that an output will not be null even if the corresponding type allows it.
/// Specifies that an input argument was not null when the call returns.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property
    | AttributeTargets.ReturnValue, Inherited = false)]
#pragma warning disable MA0182 // Avoid unused internal types
internal sealed class NotNullAttribute : Attribute
#pragma warning restore MA0182 // Avoid unused internal types
{
}
#endif
