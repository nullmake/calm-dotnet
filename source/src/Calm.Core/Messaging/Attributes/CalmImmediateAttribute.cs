#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Specifies that an event should be published immediately, bypassing the transactional Outbox (Unit of Work).
/// </summary>
/// <remarks>
/// Apply this attribute to an event type (implementing <see cref="ICalmEvent"/>) to ensure it is
/// not delayed until the end of a command execution. This is useful for progress updates or
/// real-time notifications.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class CalmImmediateAttribute : Attribute
{
}
