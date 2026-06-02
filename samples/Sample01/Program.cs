using Calm.Core;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace Sample01;

internal sealed class Program
{
    private static ILogger _logger = null!;

    static async Task Main(string[] args)
    {
        _ = args;
        Thread.CurrentThread.Name = "Main Thread";
        _logger = ConsoleLogger.Create<Program>();
        _logger.LogInformation("Application start.");

        // Creates CALM engine.
        using var calm = new CalmEngine();

        // Starts CALM engine.
        calm.Start();

        Console.CancelKeyPress += async (sender, e) =>
        {
            _logger.LogInformation("Ctrl+C pressed.");
            // When Ctrl+C pressed, signals CALM engine to stop and waits for the engine shutdown.
            e.Cancel = true;

            // Stop CALM engine.
            await calm.StopAsync();
            _logger.LogInformation("Application exit.");
            Environment.Exit(0);
        };

        _logger.LogInformation("""

            -----------------------
             Press Ctrl+C to exit.
            -----------------------
            """);
        await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

        // Start main loop.
        await calm.ScheduleAsync(ct => Loop(calm, ct));

        // Enqueue numbers.
        var random = new Random();
        for (int i = 1; i <= 20;)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            await calm.ExecuteAsync(_ =>
            {
                var numbers = Enumerable.Range(i, random.Next(0, 5)).ToArray();
                Enqueue(numbers);
                i += numbers.Length;
                return Task.CompletedTask;
            });
        }

        // Waits for the CALM engine shutdown.
        await calm.WaitForShutdownAsync();
    }

    private readonly static Queue<int> _numberQueue = new();

    private static void Enqueue(params int[] numbers)
    {
        foreach (var number in numbers)
        {
            _logger.LogInformation("Enqueue: {Number} -> Queue", number);
            _numberQueue.Enqueue(number);
        }
    }

    private static async Task Loop(ICalm calm, CancellationToken token)
    {
        // Execute a loop on the engine thread.
        _logger.LogInformation("Loop started.");
        while (!token.IsCancellationRequested)
        {
            // Note: If you do not set `ConfigureAwait(true)`, the operation will be moved to a separate thread.
            await Task.WhenAny(Task.Delay(500, token)).ConfigureAwait(true);

            // If you have switched to a different thread, use `SwitchAsync()` to return to the engine thread.
            await Task.WhenAny(Task.Delay(500, token)).ConfigureAwait(false);
            await calm.SwitchAsync();

            if (_numberQueue.Count > 0)
            {
                _logger.LogInformation("Dequeue: Queue -> {Number}", _numberQueue.Dequeue());
            }
        }
        _logger.LogInformation("Loop completed.");
    }
}
