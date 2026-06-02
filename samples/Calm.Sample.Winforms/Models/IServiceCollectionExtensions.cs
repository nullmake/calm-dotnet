using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Calm.Sample.Winforms.Models.Services.Compression;
using Calm.Sample.Winforms.Models.Services.Metrics;

namespace Calm.Sample.Winforms.Models;

/// <summary>
/// The extensions for <see cref="IServiceCollection"/>.
/// </summary>
internal static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services of The Models to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddModel(this IServiceCollection services)
    {
        services.AddTransient<Model>();
        services.AddTransientCalmHandlersFromClass<ArchiveCreatorService>();
        services.AddTransientCalmHandlersFromClass<RecompressorService>();
        services.AddTransientCalmHandlersFromClass<ResourceMonitorService>();
        return services;
    }
}
