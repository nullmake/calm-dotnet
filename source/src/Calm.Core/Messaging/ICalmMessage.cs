using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Messaging;

/// <summary>
/// A marker interface for all messages processed by CALM.
/// </summary>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for messaging")]
public interface ICalmMessage
{
}
