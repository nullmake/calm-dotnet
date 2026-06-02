#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the message handler is not registered.
/// </summary>
public class CalmNoHandlerRegisteredException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage = "No handler registered.";

    /// <summary>
    /// The message with message type.
    /// </summary>
    /// <param name="type">The type of the message or the class.</param>
    /// <returns>the formatted message.</returns>
    private static string MessageWithType(Type type)
        => $"No handler registered for type {type.Name}";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredException"/> class.
    /// </summary>
    public CalmNoHandlerRegisteredException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmNoHandlerRegisteredException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmNoHandlerRegisteredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredException"/> class.
    /// </summary>
    /// <param name="type">The type of the message or the class.</param>
    public CalmNoHandlerRegisteredException(Type type)
        : base(type is null ? _defaultMessage : MessageWithType(type))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredException"/> class.
    /// </summary>
    /// <param name="type">The type of the message or the class.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmNoHandlerRegisteredException(Type type, Exception innerException)
        : base(type is null ? _defaultMessage : MessageWithType(type),
              innerException)
    {
    }
}
