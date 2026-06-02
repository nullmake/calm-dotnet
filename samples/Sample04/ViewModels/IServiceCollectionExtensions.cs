using Calm.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Sample04.ViewModels;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddViewModel(this IServiceCollection services)
    {
        // AddScopedCalmHandlersFromClass registers the class as Scoped AND connects it to Calm engine
        services.AddScopedCalmHandlersFromClass<MainViewModel>();
        return services;
    }
}
