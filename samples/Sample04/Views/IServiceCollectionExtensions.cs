using Microsoft.Extensions.DependencyInjection;

namespace Sample04.Views;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddView(this IServiceCollection services)
    {
        services.AddScoped<MainWindow>();
        return services;
    }
}
