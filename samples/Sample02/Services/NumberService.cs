using Calm.Core;
using Microsoft.Extensions.Logging;
using Sample02.Services.Messages;
using SharedLibrary;

namespace Sample02.Services;

internal sealed class NumberService(ICalm calm) : IDisposable
{
    private readonly static ILogger _logger = ConsoleLogger.Create<NumberService>();
    private readonly ICalm _calm = calm;
    private readonly Queue<int> _numberQueue = new();
    private ScheduleOperation? scheduleOperation;
    private readonly CancellationTokenSource _loopCts = new();

    #region IDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing.");

        // Unregister the CALM handlers for this instance.
        _calm.Unregister(this);

        // Cancel the loop.
        _loopCts.Cancel();

        // Wait for the task executed via Schedule to complete
        // Note: Waiting on the CALM engine thread will cause a deadlock
        if (!_calm.IsOnEngineThread)
        {
            scheduleOperation?.CompletedAwaitable.GetAwaiter().GetResult();
        }
        _loopCts.Dispose();

        _disposed = true;
        _logger.LogInformation("Disposed.");
    }
    #endregion

    [CalmHandler]
    private Task<int> HandleGetCountQueryAsync(GetCountQuery query, CancellationToken token)
    {
        _logger.LogInformation("Handle query: {Query}", query);
        return Task.FromResult(_numberQueue.Count);
    }

    [CalmHandler]
    private async Task HandleAddCommandAsync(AddCommand command, CancellationToken token)
    {
        _logger.LogInformation("Handle command: {Command}", command);
        foreach (var number in command.Numbers)
        {
            _logger.LogInformation("Enqueue: {Number} -> Queue", number);
            _numberQueue.Enqueue(number);
        }
    }

    [CalmHandler]
    private async Task HandleStartCommandAsync(StartCommand command, CancellationToken token)
    {
        _logger.LogInformation("Handle command: {Command}", command);

        // Schedules a loop using the CALM engine.
        var operation = await _calm.ScheduleAsync(LoopAsync);
        scheduleOperation = operation;

        // Wait until the loop starts.
        await operation.StartedAwaitable;
    }

    private async Task LoopAsync(CancellationToken token)
    {
        // Execute a loop on the engine thread.
        _logger.LogInformation("Loop started.");

        // Combine with the CALM Engine shutdown token.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _loopCts.Token);
        var loopToken = cts.Token;
        while (!loopToken.IsCancellationRequested)
        {
            // Note: If you do not set `ConfigureAwait(true)`, the operation will be moved to a separate thread.
            await Task.WhenAny(Task.Delay(500, loopToken)).ConfigureAwait(true);

            // If you have switched to a different thread, use `SwitchAsync()` to return to the engine thread.
            await Task.WhenAny(Task.Delay(500, loopToken)).ConfigureAwait(false);
            await _calm.SwitchAsync();

            if (_numberQueue.Count > 0)
            {
                var number = _numberQueue.Dequeue();
                _logger.LogInformation("Dequeue: Queue -> {Number}", number);
                await _calm.Event.PublishAsync(new DequeueEvent(number, _numberQueue.Count));
            }
        }
        _logger.LogInformation("Loop completed.");
    }
}
