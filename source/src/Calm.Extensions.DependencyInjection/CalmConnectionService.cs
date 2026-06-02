using Calm.Core;
using Microsoft.Extensions.Hosting;

namespace Calm.Extensions.DependencyInjection;

/// <summary>
/// A hosted service that automatically connects all registered class instances
/// (classes with methods marked with <see cref="CalmHandlerAttribute"/>) to the
/// <see cref="ICalm"/> facade upon application startup.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmConnectionService"/> class.
/// </remarks>
/// <param name="calm">The Calm engine.</param>
internal sealed class CalmConnectionService(ICalm calm) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            calm.Start();
            await Task.WhenAny(Task.Delay(Timeout.Infinite, stoppingToken)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Do nothing.
        }
    }
}
