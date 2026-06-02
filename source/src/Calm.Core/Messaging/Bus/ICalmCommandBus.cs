using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// Represents a bus for sending commands.
/// </summary>
public interface ICalmCommandBus
{
    /// <summary>
    /// Registers a callback Handler for a specific command type that does not produce a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="callback">The callback delegate. Must be a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register<TCommand>(Func<TCommand, CancellationToken, Task> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Registers a callback handler for a specific command type that produces a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="callback">The callback delegate. Must be a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register<TCommand, TResponse>(Func<TCommand, CancellationToken, Task<TResponse>> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand<TResponse>;

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
    /// Unregisters a callback handler for a specific command type that does not produce a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="callback">The callback delegate that was previously registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister<TCommand>(Func<TCommand, CancellationToken, Task> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Unregisters a callback handler for a specific command type that produces a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="callback">The callback delegate that was previously registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister<TCommand, TResponse>(Func<TCommand, CancellationToken, Task<TResponse>> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand<TResponse>;

    /// <summary>
    /// Returns an enumerable collection of registered command handlers.
    /// </summary>
    /// <returns>An enumerable collection of registerd command handlers.</returns>
    IEnumerable<IReadOnlyMessageHandler> EnumerateMessageHandler();

    /// <summary>
    /// Returns an enumerable collection of registered command with response handlers.
    /// </summary>
    /// <returns>An enumerable collection of registerd command with response handlers.</returns>
    IEnumerable<IReadOnlyRequestHandler> EnumerateRequestHandler();

    /// <summary>
    /// Posts a command to the engine thread for asynchronous execution (Fire-and-forget).
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Post<TCommand>(TCommand command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Posts a command to the engine thread for asynchronous execution (Fire-and-forget).
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Post<TCommand>(TCommand command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Posts a command to the engine thread for asynchronous execution (Fire-and-forget).
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <remarks>NOTE: Unable to receive a response.</remarks>
    void Post<TResponse>(ICalmCommand<TResponse> command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Posts a command to the engine thread for asynchronous execution (Fire-and-forget).
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <remarks>NOTE: Unable to receive a response.</remarks>
    void Post<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Posts a command and waits for completion.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    Task PostAsync<TCommand>(TCommand command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Posts a command and waits for completion.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    Task PostAsync<TCommand>(TCommand command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Posts a command and waits for completion.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    /// <remarks>NOTE: Unable to receive a response.</remarks>
    Task PostAsync<TResponse>(ICalmCommand<TResponse> command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Posts a command and waits for completion.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    /// <remarks>NOTE: Unable to receive a response.</remarks>
    Task PostAsync<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Sends a command and waits for completion.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    Task SendAsync<TCommand>(TCommand command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Sends a command and waits for completion.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task that completes when the command has been processed.</returns>
    Task SendAsync<TCommand>(TCommand command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TCommand : notnull, ICalmCommand;

    /// <summary>
    /// Sends a command and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task<TResponse> SendAsync<TResponse>(ICalmCommand<TResponse> command,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Sends a command and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="command">The command message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task<TResponse> SendAsync<TResponse>(ICalmCommand<TResponse> command, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
}
