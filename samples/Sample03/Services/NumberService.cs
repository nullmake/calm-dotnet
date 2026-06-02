using Calm.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample03.Services.Messages;
using System.Diagnostics.CodeAnalysis;

namespace Sample03.Services;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
internal sealed class NumberService(ICalm calm, ILogger<NumberService> logger) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly ICalm _calm = calm;
    private readonly Queue<int> _numberQueue = new();

    #region IDisposable
    public override void Dispose()
    {
        _logger.LogInformation("Disposing.");
        base.Dispose();
        _logger.LogInformation("Disposed.");
    }
    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service start.");

        // Register the CALM handlers for this instance.
        _calm.Register(this);

        await _calm.ExecuteAsync(LoopAsync, stoppingToken);

        // Unregister the CALM handlers for this instance.
        _calm.Unregister(this);

        _logger.LogInformation("Service completed.");
    }

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

    private async Task LoopAsync(CancellationToken token)
    {
        // Execute a loop on the engine thread.
        _logger.LogInformation("Loop started.");
        while (!token.IsCancellationRequested)
        {
            // Note: If you do not set `ConfigureAwait(true)`, the operation will be moved to a separate thread.
            await Task.WhenAny(Task.Delay(500, token)).ConfigureAwait(true);

            // If you have switched to a different thread, use `SwitchAsync()` to return to the engine thread.
            await Task.WhenAny(Task.Delay(500, token)).ConfigureAwait(false);
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
