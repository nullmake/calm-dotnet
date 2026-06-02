#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Specifies that the dispatch logs for a message (Command, Query, or Event) should be suppressed.
/// </summary>
/// <remarks>
/// Apply this attribute to a message type to reduce log noise, especially for high-frequency messages.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class CalmSuppressLogAttribute : Attribute
{
}
