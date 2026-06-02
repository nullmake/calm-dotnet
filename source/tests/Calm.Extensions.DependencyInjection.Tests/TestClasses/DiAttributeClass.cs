using Calm.Core;
using Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;

namespace Calm.Extensions.DependencyInjection.Tests.TestClasses;

/// <summary>
/// A test handler using [CalmHandler] attribute pattern for DI testing.
/// </summary>
/// <param name="logger">The logger.</param>
internal sealed class DiAttributeClass(ILogger<DiAttributeClass> logger)
{
    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    private readonly ILogger<DiAttributeClass> _logger = logger;

    /// <summary>
    /// Gets a value indicating whether the command handler was called.
    /// </summary>
    public bool CommandWasCalled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the query handler was called.
    /// </summary>
    public bool QueryWasCalled { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the event handler was called.
    /// </summary>
    public bool EventWasCalled { get; private set; }

    /// <summary>
    /// Gets the last received command data.
    /// </summary>
    public string? LastCommandData { get; private set; }

    /// <summary>
    /// Gets the last received query data.
    /// </summary>
    public string? LastQueryData { get; private set; }

    /// <summary>
    /// Gets the last received event data.
    /// </summary>
    public string? LastEventData { get; private set; }

    /// <summary>
    /// Handles the AttributeTestCommand asynchronously.
    /// </summary>
    /// <param name="message">The command message.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the command is processed.</returns>
    /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
    [CalmHandler]
    public Task HandleAsync(TestCommand message, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("[HandleAsync] TestCommand={{ Data=\"{Data}\" }}", message.Data);
        CommandWasCalled = true;
        LastCommandData = message.Data;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the TestQuery asynchronously.
    /// </summary>
    /// <param name="message">The query message.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task containing the response string.</returns>
    /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
    [CalmHandler]
    public Task<string> HandleAsync(TestQuery message, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("[HandleAsync] TestQuery={{ Data=\"{Data}\" }}", message.Data);
        QueryWasCalled = true;
        LastQueryData = message.Data;
        return Task.FromResult($"Attribute: {message.Data}");
    }

    /// <summary>
    /// Handles the TestEvent asynchronously.
    /// </summary>
    /// <param name="message">The event message.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that completes when the event is processed.</returns>
    /// <exception cref="ArgumentNullException">The message parameter is null.</exception>
    [CalmHandler]
    public Task HandleAsync(TestEvent message, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(message);

        _logger.LogInformation("[HandleAsync] TestEvent={{ Message=\"{Message}\" }}", message.Message);
        EventWasCalled = true;
        LastEventData = message.Message;
        return Task.CompletedTask;
    }
}
