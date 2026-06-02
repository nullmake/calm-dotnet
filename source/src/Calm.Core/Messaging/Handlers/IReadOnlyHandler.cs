#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// An interface for handlers that can be matched against a delegate.
/// </summary>
public interface IReadOnlyHandler
{
    /// <summary>
    /// The callback delegate name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Determines whether the specified delegate matches the wrapped handler.
    /// </summary>
    /// <param name="method">The delegate to compare.</param>
    /// <returns>true if the method and target are the same; otherwise, false.</returns>
    bool Matches(Delegate method);
}
