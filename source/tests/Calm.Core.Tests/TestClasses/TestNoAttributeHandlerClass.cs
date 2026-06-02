using Calm.Core.Tests.TestClasses.Messages;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// Test class without CalmHandlerAttribute handlers.
/// </summary>
internal class TestNoAttributeHandlerClass
{
    /// <summary>
    /// Handles the test command (should fail registration).
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public virtual Task HandleTestCommandAsync(TestCommand command, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the test command (should fail registration).
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public virtual Task<TestResponse> HandleTestCommandWithResponseAsync(
        TestCommandWithResponse command, CancellationToken token)
    {
        return Task.FromResult(new TestResponse());
    }

    /// <summary>
    /// Handles the test query (should fail registration).
    /// </summary>
    /// <param name="query">The calm query.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public virtual Task<TestResponse> HandleTestQueryAsync(TestQuery query, CancellationToken token)
    {
        return Task.FromResult(new TestResponse());
    }

    /// <summary>
    /// Handles the test event (should fail registration).
    /// </summary>
    /// <param name="event">The calm event.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public virtual Task HandleTestEventAsync(TestEvent @event, CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
