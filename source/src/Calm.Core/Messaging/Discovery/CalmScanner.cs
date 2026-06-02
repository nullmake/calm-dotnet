using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Calm.Core.Messaging.Discovery;

/// <summary>
/// Use reflection to extract information from assemblies and classes.
/// </summary>
public static class CalmScanner
{
    /// <summary>
    /// Returns an enumerable collection of the methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly that containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <returns>An enumerable collection of the methods.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Errors are forwarded to the observer")]
    public static IEnumerable<Type> EnumerateCalmHandlerClasses(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly?.GetTypes() ?? [];
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = [.. ex.Types.OfType<Type>()];
        }

        return types.Where(type => EnumerateCalmHandlers(type).Any());
    }

    /// <summary>
    /// Returns an enumerable collection of the methods marked with <see cref="CalmHandlerAttribute"/>.
    /// </summary>
    /// <param name="instanceType">The type of instance that containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <returns>An enumerable collection of the methods.</returns>
    public static IEnumerable<CalmHandlerInfo> EnumerateCalmHandlers(Type? instanceType)
    {
        // Accept a concrete or static class.
        if ((instanceType?.IsClass) is not true || (instanceType.IsAbstract && !instanceType.IsSealed))
        {
            yield break;
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static;
        foreach (var method in instanceType.GetMethods(flags))
        {
            // Skip generic methods
            if (method.IsGenericMethod)
            {
                continue;
            }

            // Check for CalmHandler attribute
            var attribute = method.GetCustomAttribute<CalmHandlerAttribute>();
            if (attribute is null)
            {
                continue;
            }

            // Get method parameters
            var parameters = method.GetParameters();
            if (parameters.Length is 0)
            {
                continue;
            }

            yield return new CalmHandlerInfo(method);
        }
    }
}
