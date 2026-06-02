#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A non-generic interface for delegate handlers with response
/// This is a marker interface for type-safe handler management.
/// </summary>
public interface IReadOnlyRequestHandler : IReadOnlyHandler
{
    /// <summary>
    /// The request type to be handled.
    /// </summary>
    Type RequestType { get; }

    /// <summary>
    /// The response type to be handled.
    /// </summary>
    Type ResponseType { get; }
}
