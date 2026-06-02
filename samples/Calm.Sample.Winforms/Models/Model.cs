using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Calm.Sample.Winforms.Models.Services.Compression;
using Calm.Sample.Winforms.Models.Services.Metrics;

namespace Calm.Sample.Winforms.Models;

/// <summary>
/// The application model.
/// </summary>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed class Model
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance.</param>
    public Model(ILogger<Model> logger, IServiceProvider serviceProvider)
    {
        logger.LogInformation("Initializes a new instance of the {Class} class.", nameof(Model));

        // Create domain service instances.
        _ = serviceProvider.GetRequiredService<ArchiveCreatorService>();
        _ = serviceProvider.GetRequiredService<RecompressorService>();
        _ = serviceProvider.GetRequiredService<ResourceMonitorService>();
    }
}
