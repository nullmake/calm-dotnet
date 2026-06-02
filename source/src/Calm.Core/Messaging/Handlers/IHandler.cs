namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// An interface for handlers that can be matched against a delegate.
/// </summary>
internal interface IHandler : IReadOnlyHandler
{
    /// <summary>
    /// The callback handler.
    /// </summary>
    Delegate Callback { get; }
}
