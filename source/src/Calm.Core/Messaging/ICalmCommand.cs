using Calm.Core.Messaging;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a command message that is sent to a single handler and expects
/// completion acknowledgement without a return value.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for messaging")]
public interface ICalmCommand : ICalmMessage
{
}
