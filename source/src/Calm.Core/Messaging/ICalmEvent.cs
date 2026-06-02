using Calm.Core.Messaging;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a one-way message (event) that does not expect a response.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for messaging")]
public interface ICalmEvent : ICalmMessage
{
}
