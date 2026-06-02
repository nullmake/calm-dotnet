#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Marks a method as a handler for a specific message type in the CALM messaging system.
/// This attribute is used for automatic handler discovery during manual instance registration.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CalmHandlerAttribute : Attribute
{
}
