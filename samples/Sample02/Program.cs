using Calm.Core;
using Microsoft.Extensions.Logging;
using Sample02.Services;
using Sample02.Services.Messages;
using SharedLibrary;

namespace Sample02;

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

        // Create services.
        using var numberService = new NumberService(calm);

        // Register CALM handlers by the handler.
        calm.Event.Register<DequeueEvent>(HandleDequeueEventAsync);

        // Register CALM handlers by the instance.
        calm.Register(numberService);

        Console.CancelKeyPress += async (sender, e) =>
        {
            _logger.LogInformation("Ctrl+C pressed.");
            // When Ctrl+C pressed, signals CALM engine to stop and waits for the engine shutdown.
            e.Cancel = true;

            // Stop services.
            numberService.Dispose();

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
        await calm.Command.SendAsync(new StartCommand());

        // Enqueue numbers.
        var random = new Random();
        for (int i = 1; i <= 20;)
        {
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            // If the specified amount has already been reached, do not add any more.
            var count = await calm.Query.SendAsync(new GetCountQuery());
            if (count > 10)
            {
                continue;
            }

            var numbers = Enumerable.Range(i, random.Next(0, 5)).ToArray();
            i += numbers.Length;
            await calm.Command.SendAsync(new AddCommand(numbers));
        }

        // Waits for the CALM engine shutdown.
        await calm.WaitForShutdownAsync();
    }

    [CalmHandler]
    private static Task HandleDequeueEventAsync(DequeueEvent @event, CancellationToken token)
    {
        _logger.LogInformation("Handle event: {Event}", @event);
        _logger.LogInformation("Remains: {Count}", @event.Remains);
        return Task.CompletedTask;
    }
}
