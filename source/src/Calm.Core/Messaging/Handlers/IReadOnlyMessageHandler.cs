#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A non-generic interface for delegate handlers without response.
/// </summary>
public interface IReadOnlyMessageHandler : IReadOnlyHandler
{
    /// <summary>
    /// The message type to be handled.
    /// </summary>
    Type MessageType { get; }
}
