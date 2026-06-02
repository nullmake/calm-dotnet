using Calm.Core.Engines;
using Calm.Core.Messaging.Bus;
using Calm.Core.Messaging.Discovery;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging;

/// <summary>
/// Provides the CALM messaging system.
/// </summary>
internal sealed class CalmBus : ICalmBus
{
    /// <summary>
    /// The logger instance for recording diagnostic information and errors.
    /// </summary>
    private readonly CalmBusLog? _logger;

    /// <summary>
    /// The command bus responsible for command registration and dispatching.
    /// </summary>
    private readonly CalmCommandBus _commandBus;

    /// <summary>
    /// The query bus responsible for query registration and dispatching.
    /// </summary>
    private readonly CalmQueryBus _queryBus;

    /// <summary>
    /// The event bus responsible for event registration and dispatching.
    /// </summary>
    private readonly CalmEventBus _eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmBus"/> class.
    /// </summary>
    /// <param name="scheduler">The scheduler for calm engine.</param>
    /// <param name="options">The configuration options for the pump.</param>
    /// <param name="logger">The optional logger for recording diagnostic information and errors.</param>
    public CalmBus(ICalmScheduler scheduler, CalmOptions options, ILogger? logger = null)
    {
        _logger = logger is null ? null : new CalmBusLog(logger);
        var bus = new CalmBusCore(scheduler, options, _logger);
        _commandBus = new CalmCommandBus(bus, _logger);
        _queryBus = new CalmQueryBus(bus, _logger);
        _eventBus = new CalmEventBus(bus, _logger);
    }

    /// <inheritdoc/>
    public ICalmCommandBus Command => _commandBus;

    /// <inheritdoc/>
    public ICalmQueryBus Query => _queryBus;

    /// <inheritdoc/>
    public ICalmEventBus Event => _eventBus;

    /// <inheritdoc/>
    public void Register(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Register(instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
            instance, _ => true, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(object instance, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Register(instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
            instance, registrationFilter, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Register(type, null, _ => true, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Register(Type type, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Register(type, null, registrationFilter, memberName, filePath, lineNumber);

    /// <summary>
    /// Registers all static handler methods on the specified instance that are marked
    /// with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="type">The class type containing static methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="registrationFilter">Predicate that evaluates if a method should be registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    private void Register(Type type, object? instance, Func<CalmHandlerInfo, bool> registrationFilter,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var instanceName = type.Name;
            _logger?.RegisteringAllHandler(LogLevel.Trace, instanceName,
                memberName, filePath, lineNumber);

            // Use CalmScanner to discover all methods decorated with [CalmHandler].
            // This discovers both instance and static methods on the given type.
            foreach (var info in CalmScanner.EnumerateCalmHandlers(type)
                .Where(i => registrationFilter(i)))
            {
                // Determine the target instance: null for static methods, the provided instance for others.
                var target = info.IsStatic ? null : instance;

                // Dispatch the handler to the appropriate specialized bus based on its category.
                switch (info.Category)
                {
                    case CalmMessageCategory.Command:
                    case CalmMessageCategory.CommandWithMessage:
                        _commandBus.Register(info, target);
                        break;
                    case CalmMessageCategory.Query:
                        _queryBus.Register(info, target);
                        break;
                    case CalmMessageCategory.Event:
                        _eventBus.Register(info, target);
                        break;
                    default:
                        // Ignore unrecognized handler categories.
                        break;
                }
            }

            _logger?.RegisteredAllHandler(LogLevel.Information, instanceName,
                memberName, filePath, lineNumber);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Unregister(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            _logger?.WriteLine(LogLevel.Trace, "Unregistering all handler.",
                memberName, filePath, lineNumber);

            // Broadly unregister all handlers from all specialized buses.
            ((ICalmEventBus)_eventBus).Unregister(memberName, filePath, lineNumber);
            ((ICalmQueryBus)_queryBus).Unregister(memberName, filePath, lineNumber);
            ((ICalmCommandBus)_commandBus).Unregister(memberName, filePath, lineNumber);

            _logger?.WriteLine(LogLevel.Information, "Unregistered all handler.",
                memberName, filePath, lineNumber);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Unregister(object instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Unregister(instance?.GetType() ?? throw new ArgumentNullException(nameof(instance)),
            instance, memberName, filePath, lineNumber);

    /// <inheritdoc/>
    public void Unregister(Type type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        => Unregister(type, null, memberName, filePath, lineNumber);

    /// <summary>
    /// Unregisters all handler methods on the specified instance that were previously registered.
    /// </summary>
    /// <param name="type">The class type containing static methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The handler instance containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    private void Unregister(Type type, object? instance,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var instanceName = type.Name;
            _logger?.UnregisteringAllHandler(LogLevel.Trace, instanceName,
                memberName, filePath, lineNumber);

            // Re-discover handlers and remove them from their respective buses.
            foreach (var info in CalmScanner.EnumerateCalmHandlers(type))
            {
                switch (info.Category)
                {
                    case CalmMessageCategory.Command:
                    case CalmMessageCategory.CommandWithMessage:
                        _commandBus.Unregister(info, instance);
                        break;
                    case CalmMessageCategory.Query:
                        _queryBus.Unregister(info, instance);
                        break;
                    case CalmMessageCategory.Event:
                        _eventBus.Unregister(info, instance);
                        break;
                    default:
                        break;
                }
            }

            _logger?.UnregisteredAllHandler(LogLevel.Information, instanceName,
                memberName, filePath, lineNumber);
        }
        catch (Exception ex)
        {
            _logger?.Error(ex, ex.Message);
            throw;
        }
    }
}
