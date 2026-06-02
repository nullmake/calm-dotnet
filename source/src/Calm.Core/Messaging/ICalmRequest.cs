using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Messaging;

/// <summary>
/// Represents a request that expects a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for messaging")]
[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed",
    Justification = "Marker interface for messaging")]
public interface ICalmRequest<TResponse>
{
}
