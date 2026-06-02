namespace Calm.Core.Messaging.Handlers;

/// <summary>
/// Provides helper methods for the <see cref="MessageHandler{TMessage}"/> class.
/// </summary>
internal static class MessageHandler
{
    /// <summary>
    /// Creates a message handler without response.
    /// </summary>
    /// <param name="calmHandlerInfo">The information for a method marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <param name="instance">The handler instance (can be null for static methods).</param>
    /// <returns>An IMessageHandler.</returns>
    /// <exception cref="CalmSchemaException">Thrown when the method signature is invalid.</exception>
    public static IMessageHandler Create(CalmHandlerInfo calmHandlerInfo, object? instance)
    {
        var handlerType = typeof(MessageHandler<>).MakeGenericType(calmHandlerInfo.ParameterType);
        var handler = Activator.CreateInstance(handlerType, calmHandlerInfo.CreateMethod(instance));
        if (handler is not IMessageHandler messageHandler)
        {
            throw new CalmSchemaException(
                "Invalid method signature. Expected "
                + $"'Task {calmHandlerInfo.Name}({calmHandlerInfo.ParameterType.Name}, CancellationToken)'.");
        }
        return messageHandler;
    }
}
