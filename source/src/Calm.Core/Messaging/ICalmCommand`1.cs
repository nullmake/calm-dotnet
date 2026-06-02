using Calm.Core.Messaging;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a command that expects a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for messaging")]
public interface ICalmCommand<TResponse> : ICalmRequest<TResponse>
{
}
