using Calm.Core;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace Sample05;

internal sealed class Leaker
{
    private readonly ILogger _logger = ConsoleLogger.Create<Leaker>();

    public async Task ExecuteAsync(ICalm calm)
    {
        _logger.LogInformation("");
        _logger.LogInformation("************************************");
        _logger.LogInformation(" ICalmErrorObserver.OnContextLeaked");
        _logger.LogInformation("************************************");

        var contextChangedTcs = new TaskCompletionSource<bool>();

        // Execute the task using the CALM engine.
        var operation = await calm.ScheduleAsync(async ct =>
        {
            // Wait for the SynchronizationContext to be overwritten by an external source.
            await contextChangedTcs.Task.ConfigureAwait(true);

            // A context leak is detected here.
            await Task.Delay(1, ct).ConfigureAwait(true);
        });

        // Force a rewrite of the CALM engine's SynchronizationContext.
        await Task.Run(async () =>
        {
            // Switch to the CALM engine thread.
            await calm.SwitchAsync();

            // Change the current `SynchronizationContext`.
            _logger.LogInformation("Change the current `SynchronizationContext`.");
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            // Resume the task.
            contextChangedTcs.TrySetResult(true);
        }).ConfigureAwait(false);

        // Wait for the scheduled task to complete.
        await operation.CompletedAwaitable;
    }
}
