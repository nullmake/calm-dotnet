using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample04.Models.Services;

namespace Sample04.Models;

internal class Model
{
    public Model(ILogger<Model> logger, IServiceProvider serviceProvider)
    {
        logger.LogInformation("Initializes a new instance of the {Class} class.", nameof(Model));

        // Create domain service instances.
        _ = serviceProvider.GetRequiredService<ApplicationInfoService>();
    }
}
