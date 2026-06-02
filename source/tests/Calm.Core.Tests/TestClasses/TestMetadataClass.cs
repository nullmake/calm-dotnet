using Calm.Core.Tests.TestClasses.Messages;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// A handler class for testing event metadata behavior.
/// </summary>
internal sealed class TestMetadataClass
{
    /// <summary>
    /// Gets a value indicating whether the immediate event was called.
    /// </summary>
    public bool ImmediateCalled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the normal event was called.
    /// </summary>
    public bool NormalCalled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the quiet command was called.
    /// </summary>
    public bool QuietCommandCalled { get; private set; }

    /// <summary>
    /// Handles the immediate event.
    /// </summary>
    /// <param name="ev">The event.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    [CalmHandler]
    public Task HandleImmediateAsync(TestImmediateEvent ev, CancellationToken token)
    {
        ImmediateCalled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the normal test event.
    /// </summary>
    /// <param name="ev">The event.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    [CalmHandler]
    public Task HandleNormalAsync(TestEvent ev, CancellationToken token)
    {
        NormalCalled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the quiet command.
    /// </summary>
    /// <param name="cmd">The command.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    [CalmHandler]
    public Task HandleQuietCommandAsync(TestQuietCommand cmd, CancellationToken token)
    {
        QuietCommandCalled = true;
        return Task.CompletedTask;
    }
}
