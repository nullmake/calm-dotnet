namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// A class that counts the number of times a handler has been called.
/// </summary>
internal interface ITestClass : IDisposable
{
    /// <summary>
    /// Event messages to be processed.
    /// </summary>
    ICollection<string> EventMessageToBeProcessed { get; }

    /// <summary>
    /// Gets the count of handled command.
    /// </summary>
    int HandleCommandCount { get; }

    /// <summary>
    /// Gets the count of handled command with response.
    /// </summary>
    int HandleCommandWithResponseCount { get; }

    /// <summary>
    /// Gets the count of handled query.
    /// </summary>
    int HandleQueryCount { get; }

    /// <summary>
    /// Gets the count of handled event.
    /// </summary>
    int HandleEventCount { get; }

    /// <summary>
    /// Gets the TestCommandHandlerAsync delegate.
    /// </summary>
    Delegate HandleTestCommandAsync { get; }

    /// <summary>
    /// Gets the TestCommandWithResponseHandlerAsync delegate.
    /// </summary>
    Delegate HandleTestCommandWithResponseAsync { get; }

    /// <summary>
    /// Gets the TestQueryHandlerAsync delegate.
    /// </summary>
    Delegate HandleTestQueryAsync { get; }

    /// <summary>
    /// Gets the TestEventHandlerAsync delegate.
    /// </summary>
    Delegate HandleTestEventAsync { get; }

    /// <summary>
    /// Wait until there are no active handlers left.
    /// </summary>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    Task WaitUntilNoActiveHandlersAsync(CancellationToken token);
}
