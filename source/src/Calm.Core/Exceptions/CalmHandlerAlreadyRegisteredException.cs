#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the message handler is already registered.
/// </summary>
public class CalmHandlerAlreadyRegisteredException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage = "A handler is already registered.";

    /// <summary>
    /// The message with message type.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    /// <param name="addingMethodName">The method name to be register.</param>
    /// <param name="existingMethodName">The registerd method name.</param>
    /// <returns>the formatted message.</returns>
    private static string MessageWithType(Type messageType, string addingMethodName, string existingMethodName)
        => $"The method \"{existingMethodName}\" for type {messageType.Name} is already registered,"
            + $"so the method \"{addingMethodName}\" could not be registered.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
    /// </summary>
    public CalmHandlerAlreadyRegisteredException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmHandlerAlreadyRegisteredException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmHandlerAlreadyRegisteredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    /// <param name="addingMethodName">The method name to be register.</param>
    /// <param name="existingMethodName">The registerd method name.</param>
    public CalmHandlerAlreadyRegisteredException(Type messageType,
        string addingMethodName, string existingMethodName)
        : base(messageType is null
            ? _defaultMessage
            : MessageWithType(messageType, addingMethodName, existingMethodName))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    /// <param name="addingMethodName">The method name to be register.</param>
    /// <param name="existingMethodName">The registerd method name.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmHandlerAlreadyRegisteredException(Type messageType,
        string addingMethodName, string existingMethodName, Exception innerException)
        : base(messageType is null
            ? _defaultMessage
            : MessageWithType(messageType, addingMethodName, existingMethodName),
            innerException)
    {
    }
}
