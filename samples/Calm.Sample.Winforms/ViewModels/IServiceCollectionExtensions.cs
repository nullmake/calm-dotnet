using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Calm.Sample.Winforms.ViewModels;

/// <summary>
/// The extensions for <see cref="IServiceCollection"/> related to ViewModels.
/// </summary>
internal static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds ViewModel services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddViewModel(this IServiceCollection services)
    {
        // AddScopedCalmHandlersFromClass registers the class as Scoped AND connects it to Calm engine
        services.AddScopedCalmHandlersFromClass<MainViewModel>();
        services.AddScoped<AboutViewModel>();

        return services;
    }
}
