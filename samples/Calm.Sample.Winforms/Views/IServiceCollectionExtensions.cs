using Microsoft.Extensions.DependencyInjection;

namespace Calm.Sample.Winforms.Views;

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
    public static IServiceCollection AddView(this IServiceCollection services)
    {
        services.AddScoped<ViewFactory>();
        services.AddScoped<MainForm>();
        services.AddScoped<AboutForm>();
        return services;
    }
}
