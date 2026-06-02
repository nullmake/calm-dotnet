using Calm.Core;
using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample03.Services;
using Sample03.Services.Messages;
using SharedLibrary;

namespace Sample03;

internal sealed class Program
{
    private static ILogger _logger = null!;

    static async Task Main(string[] args)
    {
        // Create and configure a builder object.
        var builder = Host.CreateApplicationBuilder(args);

        // Add the logger.
        builder.Logging.AddSampleConsole(configure =>
        {
            configure.TimestampFormat = "HH:mm:ss.fff";
            configure.UseUtcTimestamp = false;
            configure.UseCategory = true;
            configure.IncludeScopes = true;
        });

        // Add CALM engine.
        builder.Services.AddCalm(configure =>
        {
            // Since the logger is passed from NET Generic Host, CALM engine logs are output.
            // In this sample, CALM engine logging is disabled.
            configure.EnableLogger = false;
        });

        // Add services.
        builder.Services.AddHostedService<NumberService>();

        // Create a Generic Host.
        var host = builder.Build();

        // Create a logger.
        _logger = host.Services.GetRequiredService<ILogger<Program>>();

        // Get a CALM instance.
        var calm = host.Services.GetRequiredService<ICalm>();

        // Register CALM handlers by the handler.
        calm.Event.Register<DequeueEvent>(HandleDequeueEventAsync);

        _ = Task.Run(async () =>
        {
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
        });

        // Run the application.
        await host.RunAsync().ConfigureAwait(false);

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
