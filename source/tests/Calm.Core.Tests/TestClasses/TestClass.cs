using Calm.Core.Tests.TestClasses.Messages;
using Microsoft.Extensions.Logging;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// A class that counts the number of times a handler has been called.
/// </summary>
/// <param name="engine">The Calm engine.</param>
/// <param name="logger">The test output helper used to write test output during execution.</param>
internal class TestClass(ICalm engine, ILogger logger) : ITestClass
{
    /// <summary>
    /// The Calm engine.
    /// </summary>
    private readonly ICalm _engine = engine;

    /// <summary>
    /// The test output helper used to write test output during execution.
    /// </summary>
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The counter of the active handlers.
    /// </summary>
    private int _activeCount;

    /// <inheritdoc/>
    public ICollection<string> EventMessageToBeProcessed { get; set; } = [];

    /// <inheritdoc/>
    public int HandleCommandCount { get; private set; }

    /// <inheritdoc/>
    public int HandleCommandWithResponseCount { get; private set; }

    /// <inheritdoc/>
    public int HandleQueryCount { get; private set; }

    /// <inheritdoc/>
    public int HandleEventCount { get; private set; }

    /// <inheritdoc/>
    Delegate ITestClass.HandleTestCommandAsync => HandleTestCommandAsync;

    /// <inheritdoc/>
    Delegate ITestClass.HandleTestCommandWithResponseAsync => HandleTestCommandWithResponseAsync;

    /// <inheritdoc/>
    Delegate ITestClass.HandleTestQueryAsync => HandleTestQueryAsync;

    /// <inheritdoc/>
    Delegate ITestClass.HandleTestEventAsync => HandleTestEventAsync;

    #region IDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">Indicates whether the object has been disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _engine.Unregister(this);
            }
            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    /// <summary>
    /// Command handler for TestCommand.
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public virtual async Task HandleTestCommandAsync(TestCommand command, CancellationToken token)
    {
        var id = Guid.NewGuid();
        try
        {
            _logger.LogInformation($"[TestCommandHandlerAsync] Starting. Command={command}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleCommandCount++;
            await command.FuncAsync(this, token);
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
            _logger.LogInformation($"[TestCommandHandlerAsync] Finished. Id={id}");
        }
    }

    /// <summary>
    /// Command handler for TestCommand.
    /// </summary>
    /// <param name="command">The calm command.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public virtual async Task<TestResponse> HandleTestCommandWithResponseAsync(
        TestCommandWithResponse command, CancellationToken token)
    {
        var id = Guid.NewGuid();
        TestResponse? response = null;
        try
        {
            _logger.LogInformation($"[TestCommandWithResponseHandlerAsync] Starting. Command={command}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleCommandWithResponseCount++;
            try
            {
                Interlocked.Increment(ref _activeCount);
                await command.FuncAsync(this, token);
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
            _logger.LogInformation($"[TestCommandWithResponseHandlerAsync] Finished. Response={response}, Id={id}");
        }
    }

    /// <summary>
    /// Query handler for TestRequest.
    /// </summary>
    /// <param name="query">The calm event.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public virtual async Task<TestResponse> HandleTestQueryAsync(TestQuery query, CancellationToken token)
    {
        var id = Guid.NewGuid();
        TestResponse? response = null;
        try
        {
            _logger.LogInformation($"[TestQueryHandlerAsync] Starting. Query={query}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            HandleQueryCount++;
            try
            {
                Interlocked.Increment(ref _activeCount);
                await query.FuncAsync(this, token);
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
            _logger.LogInformation($"[TestQueryHandlerAsync] Finished. Response={response}, Id={id}");
        }
    }

    /// <summary>
    /// Event handler for TestEvent.
    /// </summary>
    /// <param name="event">The calm event.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    public virtual async Task HandleTestEventAsync(TestEvent @event, CancellationToken token)
    {
        var id = Guid.NewGuid();
        try
        {
            _logger.LogInformation($"[TestEventHandlerAsync] Starting. Event={@event}, Id={id}");
            Interlocked.Increment(ref _activeCount);
            if (EventMessageToBeProcessed.Count is 0
                || EventMessageToBeProcessed.Contains(@event.Message))
            {
                HandleEventCount++;
                await @event.FuncAsync(this, token);
            }
        }
        finally
        {
            Interlocked.Decrement(ref _activeCount);
            _logger.LogInformation($"[TestEventHandlerAsync] Finished. Id={id}");
        }
    }

    /// <inheritdoc/>
    public async Task WaitUntilNoActiveHandlersAsync(CancellationToken token)
    {
        var id = Guid.NewGuid();
        bool canceled = false;
        try
        {
            _logger.LogInformation($"[WaitUntilNoActiveHandlers] Starting. Id={id}");
            do
            {
                canceled = token.IsCancellationRequested;
                if (canceled)
                {
                    break;
                }
                await _engine.ExecuteAsync(_ => Task.CompletedTask, token);
            }
            while (Interlocked.CompareExchange(ref _activeCount, 0, 0) > 0);
        }
        finally
        {
            _logger.LogInformation($"[WaitUntilNoActiveHandlers] Finished. Canceled={canceled}, Id={id}");
        }
    }
}
