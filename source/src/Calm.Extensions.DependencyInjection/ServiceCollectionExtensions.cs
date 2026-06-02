using Calm.Core;
using Calm.Core.Messaging.Discovery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Calm.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering CALM services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton CALM facade to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCalm(this IServiceCollection services)
        => services.AddCalm(null);

    /// <summary>
    /// Adds a singleton CALM facade to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional action to configure the CALM options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCalm(this IServiceCollection services, Action<CalmOptions>? configure)
    {
        services.TryAddSingleton<ICalm>(sp =>
        {
            var options = new CalmOptions();
            configure?.Invoke(options);

            // If ErrorObserver was not explicitly set, try to get it from DI
            options.ErrorObserver ??= sp.GetService<ICalmErrorObserver>();
            var logger = options.EnableLogger
                ? sp.GetService<ILogger<ICalm>>() ?? sp.GetRequiredService<ILogger>()
                : null;

            return new CalmEngine(options, logger);
        });

        // Ensure the connection service is registered
        services.AddHostedService<CalmConnectionService>();
        return services;
    }

    #region Register by assembly.
    /// <summary>
    /// Scans the specified assembly for classes with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handlers.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either service or assembly is null.</exception>
    public static IServiceCollection AddCalmHandlerClassesFromAssembly(this IServiceCollection services,
        ServiceLifetime lifetime, Assembly assembly)
        => services.AddCalmHandlersFromAssembly(lifetime, assembly, _ => true);

    /// <summary>
    /// Scans the specified assembly for classes with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handlers.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either service or assembly is null.</exception>
    public static IServiceCollection AddCalmHandlersFromAssembly(this IServiceCollection services,
        ServiceLifetime lifetime, Assembly assembly, Func<CalmHandlerInfo, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var classType in CalmScanner.EnumerateCalmHandlerClasses(assembly))
        {
            services.AddCalmHandlersFromClass(lifetime, classType, filter);
        }
        return services;
    }
    #endregion

    #region Register by class.
    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as transient lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientCalmHandlersFromClass<TClass>(this IServiceCollection services)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Transient, _ => true);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as transient lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTransientCalmHandlersFromClass<TClass>(this IServiceCollection services,
        Func<CalmHandlerInfo, bool> filter)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Transient, filter);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as scoped lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedCalmHandlersFromClass<TClass>(this IServiceCollection services)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Scoped, _ => true);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as scoped lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddScopedCalmHandlersFromClass<TClass>(this IServiceCollection services,
        Func<CalmHandlerInfo, bool> filter)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Scoped, filter);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as singleton lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonCalmHandlersFromClass<TClass>(this IServiceCollection services)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Singleton, _ => true);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them as singleton lifetime with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSingletonCalmHandlersFromClass<TClass>(this IServiceCollection services,
        Func<CalmHandlerInfo, bool> filter)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(ServiceLifetime.Singleton, filter);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handler.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCalmHandlersFromClass<TClass>(this IServiceCollection services,
        ServiceLifetime lifetime)
        where TClass : class
        => services.AddCalmHandlersFromClass<TClass>(lifetime, _ => true);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <typeparam name="TClass">
    /// The class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handler.</param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCalmHandlersFromClass<TClass>(this IServiceCollection services,
        ServiceLifetime lifetime, Func<CalmHandlerInfo, bool> filter)
        where TClass : class
        => services.AddCalmHandlersFromClass(lifetime, typeof(TClass), filter);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handler.</param>
    /// <param name="classType">
    /// The type of the class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either service or classType is null.</exception>
    /// <exception cref="CalmNoHandlerRegisteredException">No registrable handlers.</exception>
    public static IServiceCollection AddCalmHandlersFromClass(this IServiceCollection services,
        ServiceLifetime lifetime, Type classType)
        => services.AddCalmHandlersFromClass(lifetime, classType, _ => true);

    /// <summary>
    /// Scans the specified class with methods marked with <see cref="CalmHandlerAttribute"/>
    /// and registers them with the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime for the handler.</param>
    /// <param name="classType">
    /// The type of the class containing methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </param>
    /// <param name="filter">Predicate that evaluates if a method
    /// with <see cref="CalmHandlerAttribute"/> should be registered.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Either service or classType is null.</exception>
    /// <exception cref="CalmNoHandlerRegisteredException">No registrable handlers.</exception>
    public static IServiceCollection AddCalmHandlersFromClass(this IServiceCollection services,
        ServiceLifetime lifetime, Type classType, Func<CalmHandlerInfo, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(classType);

        // Validate class type
        if (!CalmScanner.EnumerateCalmHandlers(classType).Any())
        {
            throw new CalmNoHandlerRegisteredException(classType);
        }

        // Register the concrete type
        var descriptor = ServiceDescriptor.Describe(classType, sp =>
        {
            var instance = ActivatorUtilities.CreateInstance(sp, classType);
            sp.GetRequiredService<ICalm>().Register(instance, filter);
            return instance;
        }, lifetime);
        services.TryAdd(descriptor);

        return services;
    }
    #endregion
}
