namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// Provides helper methods for the <see cref="RequestHandler{TRequest, TResponse}"/> class.
/// </summary>
internal static class RequestHandler
{
    /// <summary>
    /// Creates a request handler.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The handler instance (can be null for static methods).</param>
    /// <returns>An IRequestHandler.</returns>
    /// <exception cref="CalmSchemaException">Thrown when the method signature is invalid.</exception>
    public static IRequestHandler Create(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        var requestType = calmHandlerInfo.ParameterType;
        var responseValueType = calmHandlerInfo.ReturnValueType;

        var handlerType = typeof(RequestHandler<,>).MakeGenericType(requestType, responseValueType);
        var handler = Activator.CreateInstance(handlerType, calmHandlerInfo.CreateMethod(instance));
        if (handler is not IRequestHandler requestHandler)
        {
            var methodName = calmHandlerInfo.Name;
            throw new CalmSchemaException(
                "Invalid method signature. Expected "
                + $"'Task<{responseValueType.Name}> {methodName}({requestType.Name}, CancellationToken)'.");
        }
        return requestHandler;
    }
}
