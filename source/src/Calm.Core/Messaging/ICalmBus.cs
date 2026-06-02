using Calm.Core.Messaging.Bus;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging;

/// <summary>
/// Represents a unified message bus that combines command, query, and event bus functionalities.
/// For better adherence to the Interface Segregation Principle, it is recommended to request
/// the more specific interfaces (ICalmCommandBus, ICalmQueryBus, ICalmEventBus) via dependency injection where possible.
/// </summary>
public interface ICalmBus
{
    /// <summary>
    /// Registers all handler methods on the specified instance that are marked
    /// with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="instance">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Registers all handler methods on the specified instance that are marked
    /// with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="instance">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="registrationFilter">Predicate that evaluates if a method should be registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register(object instance, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Registers all static handler methods on the specified instance that are marked
    /// with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="type">The class type containing static methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Registers all static handler methods on the specified instance that are marked
    /// with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="type">The class type containing static methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="registrationFilter">Predicate that evaluates if a method should be registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register(Type type, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Unregisters all handler methods.
    /// </summary>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Unregisters all handler methods on the specified instance that were previously registered.
    /// </summary>
    /// <param name="instance">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Unregisters all handler methods on the specified instance that were previously registered.
    /// </summary>
    /// <param name="type">The class type containing static methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Gets the command bus for sending commands.
    /// </summary>
    ICalmCommandBus Command { get; }

    /// <summary>
    /// Gets the query bus for sending queries.
    /// </summary>
    ICalmQueryBus Query { get; }

    /// <summary>
    /// Gets the event bus for publishing events.
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Intentional naming aligned with the Command/Query/Event messaging pattern.")]
    ICalmEventBus Event { get; }
}
