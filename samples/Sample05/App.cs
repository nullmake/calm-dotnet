using Calm.Core;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace Sample05;

internal sealed class App : ICalmErrorObserver
{
    private ILogger _logger = default!;

    public async Task RunAsync(string[] args)
    {
        _ = args;
        Thread.CurrentThread.Name = "Main Thread";
        _logger = ConsoleLogger.Create<Program>();
        _logger.LogInformation("Application start.");

        // Create CALM engine.
        using var calm = new CalmEngine(new CalmOptions
        {
            ErrorObserver = this,
            WatchdogThreshold = TimeSpan.FromSeconds(1)
        });

        // Start CALM engine.
        calm.Start();

        // Setup and execute the class that leaks context.
        var leaker = new Leaker();
        await leaker.ExecuteAsync(calm).ConfigureAwait(false);

        // Setup and execute the class that executes the long-run task segument.
        var staller = new Staller();
        await staller.ExecuteAsync(calm).ConfigureAwait(false);

        // Setup and execute the class that throws the exception.
        var thrower = new Thrower();
        calm.Register(thrower);
        await thrower.ExecuteAsync(calm).ConfigureAwait(false);

        // Stop CALM engine.
        await calm.StopAsync();
        _logger.LogInformation("Application exit.");
    }

    #region ICalmErrorObserver
    public void OnContextLeaked()
    {
        _logger.LogWarning("Although a context leak has been detected, the engine restores the context.");
    }

    public void OnStall(StallEventArgs e)
    {
        _logger.LogWarning("An engine stall is detected: {Event}", e);
    }

    public void OnUnhandledException(Exception exception)
    {
        _logger.LogError("{Message}", exception.Message);
    }
    #endregion
}
