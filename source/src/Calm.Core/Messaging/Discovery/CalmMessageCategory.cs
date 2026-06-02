#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Category of Calm Messages
/// </summary>
public enum CalmMessageCategory
{
    /// <summary>
    /// A type that implements <see cref="ICalmCommand"/>.
    /// </summary>
    Command,

    /// <summary>
    /// A type that implements <see cref="ICalmCommand{TResponse}"/>.
    /// </summary>
    CommandWithMessage,

    /// <summary>
    /// A type that implements <see cref="ICalmQuery{TResponse}"/>.
    /// </summary>
    Query,

    /// <summary>
    /// A type that implements <see cref="ICalmEvent"/>.
    /// </summary>
    Event,
}
