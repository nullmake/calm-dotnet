using Calm.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample04.Models.Bus.Commands;
using Sample04.Models.Bus.Events;
using SharedLibrary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Sample04.Models.Services;

internal sealed class ResourceMonitorService(ICalm calm, ILogger<ResourceMonitorService> logger) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly ICalm _calm = calm;
    private TimeSpan _SamplingPeriod = TimeSpan.FromSeconds(1);
    private CancellationTokenSource _delayCts = new();

    #region IDisposable
    private bool _disposed;

    public override void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogInformation("Disposing.");

            // Unregister the CALM handlers for this instance.
            _calm.Unregister(this);

            _delayCts.Dispose();

            _disposed = true;
            _logger.LogInformation("Disposed.");
        }
        base.Dispose();
    }
    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Service start.");

        // Register the CALM handlers for this instance.
        _calm.Register(this);

        // Executing a loop on the CALM engine.
        await _calm.ExecuteAsync(LoopAsync, stoppingToken);

        _logger.LogInformation("Service completed.");
    }

    [CalmHandler]
    private async Task HandleChangeSamplingPeriodCommandAsync(ChangeSamplingPeriodCommand command, CancellationToken token)
    {
        _logger.LogInformation("Handle command: {Command}", command);

        var period = command.Period;
        if (period < TimeSpan.FromMilliseconds(100))
        {
            throw new ArgumentOutOfRangeException(nameof(command),
                $"Please set a value of 100 ms or more. ({nameof(command.Period)}={period.TotalSeconds:0.000})");
        }

        _SamplingPeriod = period;
#if NETFRAMEWORK
        _delayCts.Cancel();
#else
        await _delayCts.CancelAsync();
#endif
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    private async Task LoopAsync(CancellationToken token)
    {
        _logger.LogInformation("Loop started.");
        try
        {
            using var process = Process.GetCurrentProcess();
            var delta = new Delta<DateTimeOffset, TimeSpan>(DateTimeOffset.UtcNow);
            var deltaProcessorTime = new Delta<TimeSpan, TimeSpan>(process.TotalProcessorTime);
            var processorCount = Environment.ProcessorCount;

            // Execute a loop on the engine thread.
            while (!token.IsCancellationRequested)
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token, _delayCts.Token))
                {
                    var task = await Task.WhenAny(Task.Delay(_SamplingPeriod, cts.Token)).ConfigureAwait(true);
                    if (task.IsCanceled)
                    {
                        if (_delayCts.IsCancellationRequested)
                        {
                            _logger.LogInformation("Reset sampling period.");
                            _delayCts.Dispose();
                            _delayCts = new();
                        }
                        continue;
                    }
                }

                process.Refresh();
                delta.SetValue(DateTimeOffset.UtcNow);
                deltaProcessorTime.SetValue(process.TotalProcessorTime);

                await _calm.Event.PublishAsync(new ProcessResourceUpdatedEvent
                {
                    CpuUsage = deltaProcessorTime.Value.TotalSeconds / delta.Value.TotalSeconds / processorCount,
                    PrivateBytes = process.PrivateMemorySize64,
                    WorkingSet = process.WorkingSet64,
                    VirtualMermory = process.VirtualMemorySize64,
                    HandleCount = process.HandleCount,
                    GcHeapSize = GC.GetTotalMemory(forceFullCollection: false),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error has occurred.");
        }
        finally
        {
            _logger.LogInformation("Loop completed.");
        }
    }
}
