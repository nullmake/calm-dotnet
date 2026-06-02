using Calm.Core;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace Sample05;

internal sealed class Staller
{
    private readonly ILogger _logger = ConsoleLogger.Create<Staller>();

    private void PutSeparator()
        => _logger.LogInformation("{Separator}", new string('-', 32));

    public async Task ExecuteAsync(ICalm calm)
    {
        _logger.LogInformation("");
        _logger.LogInformation("****************************");
        _logger.LogInformation(" ICalmErrorObserver.OnStall");
        _logger.LogInformation("****************************");

        // Execute the long-run task segument using the CALM engine.
        await calm.ExecuteAsync(async ct =>
        {
            PutSeparator();
            Sleep(TimeSpan.FromSeconds(0.1), calm.Options.WatchdogThreshold);
            await Task.Yield();

            PutSeparator();
            Sleep(TimeSpan.FromSeconds(0.5), calm.Options.WatchdogThreshold);
            await Task.Yield();

            PutSeparator();
            Sleep(TimeSpan.FromSeconds(1.1), calm.Options.WatchdogThreshold);
            await Task.Yield();

            PutSeparator();
            Sleep(TimeSpan.FromSeconds(0.8), calm.Options.WatchdogThreshold);
            await Task.Yield();
        });
    }

    private void Sleep(TimeSpan timeout, TimeSpan watchdogThreshold)
    {
        if (timeout > watchdogThreshold)
        {
            _logger.LogInformation("An engine stall will be detected: Segment={Time}, Threshold={Threshold}",
                timeout, watchdogThreshold);
        }
        else
        {
            _logger.LogInformation("An engine stall will not be detected: Segment={Time}, Threshold={Threshold}",
                timeout, watchdogThreshold);
        }
        Thread.Sleep(timeout);
    }
}
