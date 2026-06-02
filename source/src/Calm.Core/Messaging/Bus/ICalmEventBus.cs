using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// Represents a bus for publishing events.
/// </summary>
public interface ICalmEventBus
{
    /// <summary>
    /// Registers a callback handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="callback">The handler delegate. Must be a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register<TEvent>(Func<TEvent, CancellationToken, Task> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;

    /// <summary>
    /// Unregisters all callback handler.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Unregisters a callback handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="callback">The handler delegate that was previously registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister<TEvent>(Func<TEvent, CancellationToken, Task> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;

    /// <summary>
    /// Returns an enumerable collection of registered event handlers.
    /// </summary>
    /// <returns>An enumerable collection of registerd command handlers.</returns>
    IEnumerable<IReadOnlyMessageHandler> EnumerateMessageHandler();

    /// <summary>
    /// Publishes an event to all registered handlers for the event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Intentional naming aligned with the Command/Query/Event messaging pattern.")]
    void Publish<TEvent>(TEvent @event,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;

    /// <summary>
    /// Publishes an event to all registered handlers for the event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Intentional naming aligned with the Command/Query/Event messaging pattern.")]
    void Publish<TEvent>(TEvent @event, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;

    /// <summary>
    /// Publishes an event and waits for all handlers to complete.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when all event handlers have finished processing.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Intentional naming aligned with the Command/Query/Event messaging pattern.")]
    Task PublishAsync<TEvent>(TEvent @event,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;

    /// <summary>
    /// Publishes an event and waits for all handlers to complete.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event.</typeparam>
    /// <param name="event">The event message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when all event handlers have finished processing.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Intentional naming aligned with the Command/Query/Event messaging pattern.")]
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TEvent : notnull, ICalmEvent;
}
