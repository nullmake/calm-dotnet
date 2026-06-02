using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Sample04.Models.Services;

namespace Sample04.Models;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddModel(this IServiceCollection services)
    {
        // A class for creating multiple instances at once.
        services.AddTransient<Model>();
        services.AddTransientCalmHandlersFromClass<ApplicationInfoService>();

        // For IHostedService, since it is initialized using the .NET Generic Host,
        // register it using AddHostedService.
        services.AddHostedService<ResourceMonitorService>();
        return services;
    }
}
