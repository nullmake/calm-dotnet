using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// The information for a method marked with <see cref="CalmHandlerAttribute"/>.
/// </summary>
public record CalmHandlerInfo
{
    /// <summary>
    /// The category of the Calm message and method.
    /// </summary>
    public CalmMessageCategory Category { get; }

#pragma warning disable CA1034 // Nested types should not be visible
    /// <summary>
    /// The fallback type when `DeclaringType` is null.
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S2094:Classes should not be empty",
        Justification = "This is a fallback class used when the `DeclaringType` is null.")]
    public static class UnknownDeclaringType
    {
    }
#pragma warning restore CA1034 // Nested types should not be visible

    /// <summary>
    /// Gets the class that declares this method.
    /// </summary>
    public Type DeclaringType => MethodInfo.DeclaringType ?? typeof(UnknownDeclaringType);

    /// <summary>
    /// Gets a value indicating whether the method is static
    /// </summary>
    public bool IsStatic => MethodInfo.IsStatic;

    /// <summary>
    /// Gets the name of the handler method.
    /// </summary>
    public string Name => MethodInfo.Name;

    /// <summary>
    /// The method information.
    /// </summary>
    public MethodInfo MethodInfo { get; }

    /// <summary>
    /// The type of parameter processed by the method.
    /// </summary>
    public Type ParameterType { get; }

    /// <summary>
    /// The type of parameter processed by the method.
    /// </summary>
    public Type ReturnType => MethodInfo.ReturnType;

    /// <summary>
    /// The return type by the method.
    /// If the return type is <see langword="Task"/>, the value is <see langword="void"/>.
    /// If it is <see langword="Task&lt;T&gt;"/>, the value is <see langword="T"/>.
    /// </summary>
    public Type ReturnValueType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerInfo"/> class.
    /// </summary>
    /// <param name="methodInfo">The method information.</param>
    public CalmHandlerInfo(MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        var parameters = methodInfo.GetParameters();
        if (parameters.Length < 2)
        {
            throw new CalmSchemaException(
                $"Method '{methodInfo.Name}' expects 2 parameters"
                + $", but {parameters.Length} were provided.");
        }
        var secondParameterType = parameters[1].ParameterType;
        if (secondParameterType != typeof(CancellationToken))
        {
            throw new CalmSchemaException(
                $"The second parameter of method '{methodInfo.Name}' must be of type 'CancellationToken',"
                + $"but '{secondParameterType}' was found.");
        }

        var firstParameterType = parameters[0].ParameterType;
        var (category, returnValueType) = GetCategoryFrom(methodInfo, firstParameterType);

        MethodInfo = methodInfo;
        ParameterType = firstParameterType;
        Category = category;
        ReturnValueType = returnValueType;
    }

    /// <summary>
    /// Retrieve both the category and the return value type.
    /// </summary>
    /// <param name="methodInfo">The method information.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <returns>the category and the return value type.</returns>
    /// <exception cref="CalmSchemaException">the method schema violates rules</exception>
    private static (CalmMessageCategory category, Type returnValueType)
        GetCategoryFrom(MethodInfo methodInfo, Type parameterType)
    {
        var returnType = methodInfo.ReturnType;
        if (returnType == typeof(Task))
        {
            if (typeof(ICalmEvent).IsAssignableFrom(parameterType))
            {
                return (CalmMessageCategory.Event, typeof(void));
            }
            if (typeof(ICalmCommand).IsAssignableFrom(parameterType))
            {
                return (CalmMessageCategory.Command, typeof(void));
            }
            throw new CalmSchemaException(
                $"\"Task {methodInfo.Name}({parameterType.Name}, CancellationToken)\""
                + ": Parameter type must implement ICalmCommand or ICalmEvent.");
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var returnValueType = returnType.GetGenericArguments()[0];
            foreach (var interfaceType in parameterType.GetInterfaces())
            {
                if (interfaceType.IsGenericType)
                {
                    var genericType = interfaceType.GetGenericTypeDefinition();
                    if (genericType == typeof(ICalmQuery<>))
                    {
                        return (CalmMessageCategory.Query, returnValueType);
                    }
                    if (genericType == typeof(ICalmCommand<>))
                    {
                        return (CalmMessageCategory.CommandWithMessage, returnValueType);
                    }
                }
            }
            throw new CalmSchemaException(
                $"\"Task<{returnValueType}> {methodInfo.Name}({parameterType.Name}, CancellationToken)\""
                + ": Parameter type must implement ICalmCommand<> or ICalmQuery<>.");
        }

        throw new CalmSchemaException(
            $"\"{returnType.Name} {methodInfo.Name}({parameterType.Name}, CancellationToken)\""
            + ": Return type  must implement Task or Task<>.");
    }

    /// <summary>
    /// Create a delegate.
    /// </summary>
    /// <param name="instance">If an instance is specified, create a method for that instance.
    /// If null is specified, create a static method for the class.</param>
    /// <returns>The handler method.</returns>
    public Delegate CreateMethod(object? instance)
    {
        var callbackType = typeof(Func<,,>)
            .MakeGenericType(ParameterType, typeof(CancellationToken), MethodInfo.ReturnType);
        return MethodInfo.CreateDelegate(callbackType, IsStatic ? null : instance);
    }
}
