using System.Runtime.CompilerServices;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// Represents a bus for sending queries.
/// </summary>
public interface ICalmQueryBus
{
    /// <summary>
    /// Registers a callback handler for a specific query type.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="callback">The callback delegate. Must be a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Register<TQuery, TResponse>(Func<TQuery, CancellationToken, Task<TResponse>> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TQuery : notnull, ICalmQuery<TResponse>;

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
    /// Unregisters a callback handler for a specific query type.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="callback">The callback delegate that was previously registered.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    void Unregister<TQuery, TResponse>(Func<TQuery, CancellationToken, Task<TResponse>> callback,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
        where TQuery : notnull, ICalmQuery<TResponse>;

    /// <summary>
    /// Returns an enumerable collection of registered query handlers.
    /// </summary>
    /// <returns>An enumerable collection of registerd query handlers.</returns>
    IEnumerable<IReadOnlyRequestHandler> EnumerateRequestHandler();

    /// <summary>
    /// Sends a query and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="query">The query message.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task<TResponse> SendAsync<TResponse>(ICalmQuery<TResponse> query,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    /// <summary>
    /// Sends a query and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="query">The query message.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <param name="memberName">Automatically populated caller member name.</param>
    /// <param name="filePath">Automatically populated caller file path.</param>
    /// <param name="lineNumber">Automatically populated caller line number.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task<TResponse> SendAsync<TResponse>(ICalmQuery<TResponse> query, CancellationToken token,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);
}
