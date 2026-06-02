using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms.Views;

/// <summary>
/// The factory class for View.
/// </summary>
/// <param name="ServiceProvider">The <see cref="IServiceProvider"/> instance.</param>
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes",
    Justification = "Create via DI container.")]
internal sealed class ViewFactory(IServiceProvider ServiceProvider)
{
    /// <summary>
    /// Creates a specified type of View.
    /// </summary>
    /// <typeparam name="T">The type of View</typeparam>
    /// <returns>The View instance.</returns>
    /// <exception cref="KeyNotFoundException">The type has not been found.</exception>
    public T Create<T>()
        => (T)(ServiceProvider.GetService(typeof(T))
            ?? throw new KeyNotFoundException($"{nameof(T)} has not been found."));

    /// <summary>
    /// Creates a new <see cref="IServiceScope"/> that can be used to resolve scoped services.
    /// </summary>
    /// <returns>The <see cref="IServiceScope"/> instance.</returns>
    public IServiceScope CreateScope()
        => ServiceProvider.CreateScope();

    /// <summary>
    /// Creates a new <see cref="AsyncServiceScope"/> that can be used to resolve scoped services.
    /// </summary>
    /// <returns>The <see cref="AsyncServiceScope"/> instance.</returns>
    public AsyncServiceScope CreateAsyncScope()
        => ServiceProvider.CreateAsyncScope();
}
