using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// A class that counts the number of times a handler has been called.
/// </summary>
internal static class TestStaticHandlerClass
{
    /// <summary>
    /// The Calm engine.
    /// </summary>
    public static ICalm? Engine { get; set; }

    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    public static ILogger? Logger { get; set; }

    /// <summary>
    /// The counter of the active handlers.
    /// </summary>
    private static int _activeCount;

    /// <summary>
    /// Event messages to be processed.
    /// </summary>
    public static ICollection<string> EventMessageToBeProcessed { get; set; } = [];

    /// <summary>
    /// Gets the count of handled command.
    /// </summary>
    public static int HandleCommandCount { get; private set; }

    /// <summary>
    /// Gets the count of handled command with response.
    /// </summary>
    public static int HandleCommandWithResponseCount { get; private set; }

    /// <summary>
    /// Gets the count of handled query.
    /// </summary>
    public static int HandleQueryCount { get; private set; }

    /// <summary>
    /// Gets the count of handled event.
    /// </summary>
    public static int HandleEventCount { get; private set; }

    /// <summary>
    /// A semaphore for mutual exclusion.
    /// </summary>
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Cleanup static class.
    /// </summary>
    private static void Cleanup()
    {
        Engine = null;
        Logger = null;
        _activeCount = 0;
        EventMessageToBeProcessed = [];
        HandleCommandCount = 0;
        HandleCommandWithResponseCount = 0;
        HandleQueryCount = 0;
        HandleEventCount = 0;
    }

    /// <summary>
    /// Set up this static class.
    /// </summary>
    /// <param name="engine">The Calm engine.</param>
    /// <param name="logger">The test output helper used to write test output during execution.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public static async Task SetupAsync(ICalm engine, ILogger logger)
    {
        await _semaphore.WaitAsync(TestContext.Current.CancellationToken);
        Cleanup();
        Engine = engine;
        Logger = logger;
    }

    /// <summary>
    /// Tear down this static class.
    /// </summary>
    public static void Teardown()
    {
        Cleanup();
        _semaphore.Release();
    }

    /// <summary>
    /// Command handler for TestCommand.
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public static async Task HandleTestCommandAsync(TestCommand command, CancellationToken token)
    {
        var id = Guid.NewGuid();
        try
        {
            Logger?.LogInformation($"[TestCommandHandlerAsync] Starting. Command={command}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleCommandCount++;
            await command.FuncAsync(new object(), token);
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
        }
    }

    /// <summary>
    /// Command handler for TestCommand.
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public static async Task<TestResponse> HandleTestCommandWithResponseAsync(
        TestCommandWithResponse command, CancellationToken token)
    {
        var id = Guid.NewGuid();
        TestResponse? response = null;
        try
        {
            Logger?.LogInformation($"[TestCommandWithResponseHandlerAsync] Starting. Command={command}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleCommandWithResponseCount++;
            try
            {
                Interlocked.Increment(ref _activeCount);
                await command.FuncAsync(new object(), token);
            }
            finally
            {
                Interlocked.Decrement(ref _activeCount);
            }
            response = new TestResponse(command.Output);
            return response;
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
            Logger?.LogInformation($"[TestCommandWithResponseHandlerAsync] Finished. Response={response}, Id={id}");
        }
    }

    /// <summary>
    /// Query handler for TestRequest.
    /// </summary>
    /// <param name="query">The calm event.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public static async Task<TestResponse> HandleTestQueryAsync(TestQuery query, CancellationToken token)
    {
        var id = Guid.NewGuid();
        TestResponse? response = null;
        try
        {
            Logger?.LogInformation($"[TestQueryHandlerAsync] Starting. Query={query}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleQueryCount++;
            try
            {
                Interlocked.Increment(ref _activeCount);
                await query.FuncAsync(new object(), token);
            }
            finally
            {
                Interlocked.Decrement(ref _activeCount);
            }
            response = new TestResponse(query.Output);
            return response;
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
            Logger?.LogInformation($"[TestQueryHandlerAsync] Finished. Response={response}, Id={id}");
        }
    }

    /// <summary>
    /// Event handler for TestEvent.
    /// </summary>
    /// <param name="event">The calm event.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public static async Task HandleTestEventAsync(TestEvent @event, CancellationToken token)
    {
        var id = Guid.NewGuid();
        try
        {
            Logger?.LogInformation($"[TestEventHandlerAsync] Starting. Event={@event}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            if (EventMessageToBeProcessed.Count is 0
                || EventMessageToBeProcessed.Contains(@event.Message))
            {
                HandleEventCount++;
                await @event.FuncAsync(new object(), token);
            }
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
            Logger?.LogInformation($"[TestEventHandlerAsync] Finished. Id={id}");
        }
    }

    /// <summary>
    /// Wait until there are no active handlers left.
    /// </summary>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    public static async Task WaitUntilNoActiveHandlersAsync(CancellationToken token)
    {
        var id = Guid.NewGuid();
        bool canceled = false;
        try
        {
            Logger?.LogInformation($"[WaitUntilNoActiveHandlers] Starting. Id={id}");
            do
            {
                canceled = token.IsCancellationRequested;
                if (canceled)
                {
                    break;
                }
                if (Engine is not null)
                {
                    await Engine.ExecuteAsync(_ => Task.CompletedTask, token);
                }
            }
            while (Interlocked.CompareExchange(ref _activeCount, 0, 0) > 0);
        }
        finally
        {
            Logger?.LogInformation($"[WaitUntilNoActiveHandlers] Finished. Canceled={canceled}, Id={id}");
        }
    }
}
