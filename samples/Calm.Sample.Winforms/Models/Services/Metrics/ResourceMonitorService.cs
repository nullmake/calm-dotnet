using Calm.Core;
using Calm.Sample.Winforms.Infrastructure.Application;
using Calm.Sample.Winforms.Models.Bus.Commands;
using Calm.Sample.Winforms.Models.Bus.Events;
using Calm.Sample.Winforms.Models.Bus.Queries;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms.Models.Services.Metrics;

/// <summary>
/// The resource monitor for the current application.
/// </summary>
/// <param name="logger">The logger instance.</param>
/// <param name="calm">The calm engine instance.</param>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed class ResourceMonitorService(ILogger<ResourceMonitorService> logger, ICalm calm) : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The logger instance.
    /// </summary>
    private readonly ILogger _logger = logger;

    /// <summary>
    /// The calm engine instance.
    /// </summary>
    private readonly ICalm _calm = calm;

    /// <summary>
    /// The source for cancelling the monitoring loop.
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// The event for completion of the monitoring loop.
    /// </summary>
    private CalmAwaitable? _completedAwaitable;

    #region IDisposable, IAsyncDisposable
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <inheritdoc/>
    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "To wait for the loop to complete.")]
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing '{Class}' instance.", nameof(ResourceMonitorService));
        _calm.Unregister(this);
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            if (_completedAwaitable is not null)
            {
                if (!_calm.IsOnEngineThread)
                {
                    _completedAwaitable.Value.GetAwaiter().GetResult();
                }
            }
            _cancellationTokenSource.Dispose();
        }
        _disposed = true;
    }

    /// <inheritdoc/>
    [SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks",
        Justification = "To wait for the loop to complete.")]
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _logger.LogInformation("Disposing '{Class}' instance.", nameof(ResourceMonitorService));
        _calm.Unregister(this);
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            if (_completedAwaitable is not null)
            {
                if (!_calm.IsOnEngineThread)
                {
                    await _completedAwaitable.Value.ConfigureAwait(false);
                }
            }
            _cancellationTokenSource.Dispose();
        }
        _disposed = true;
    }
    #endregion

    /// <summary>
    /// Handles the <see cref="GetSystemResourceQuery"/>.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private Task<ProcessPerformanceSample> HandleGetSystemResourceQueryAsync(
        GetSystemResourceQuery query, CancellationToken token)
    {
        _logger.LogInformation("Handle query: {Query}", query);
        return Task.FromResult(CurrentApplication.Performance.Sample());
    }

    /// <summary>
    /// Handles the <see cref="StartMonitoringSystemResourceCommand"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="_">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    private async Task<bool> HandleStartMonitoringSystemResourceCommandAsync(
        StartMonitoringSystemResourceCommand command, CancellationToken _)
    {
        _logger.LogInformation("Handle comand: {Command}", command);
        if (_cancellationTokenSource is not null)
        {
            return false;
        }
        _cancellationTokenSource = new();
        var operation = await _calm.ScheduleAsync(async ct =>
        {
            await MonitorAsync(command.SamplingPeriod, ct);
        }, _cancellationTokenSource.Token);
        _completedAwaitable = operation.CompletedAwaitable;
        return true;
    }

    /// <summary>
    /// Handles the <see cref="StopMonitoringSystemResourceCommand"/>.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [CalmHandler]
    [SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks",
        Justification = "To wait for the loop to complete.")]
    private async Task HandleStopMonitoringSystemResourceCommandAsync(
        StopMonitoringSystemResourceCommand command, CancellationToken token)
    {
        _logger.LogInformation("Handle comand: {Command}", command);
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            if (_completedAwaitable is not null)
            {
                await _completedAwaitable.Value;
                _completedAwaitable = null;
            }
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Monitors the system resources of the current application.
    /// </summary>
    /// <param name="period">The sampling period.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task MonitorAsync(TimeSpan period, CancellationToken token)
    {
        try
        {
            logger.LogInformation("Start monitoring the system resources of the current application.");
            var perf = CurrentApplication.Performance;
            while (!token.IsCancellationRequested)
            {
                var ev = new UpdatedSystemResourceEvent(perf.Sample());
                await _calm.Event.PublishAsync(ev, token);
                await Task.Delay(period, _calm.Options.TimeProvider, token).ConfigureAwait(true);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            throw;
        }
        _logger.LogInformation("Stopped monitoring the system resources of the current application.");
    }
}
