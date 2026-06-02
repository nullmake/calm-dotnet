#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the null message handler will be registering.
/// </summary>
public class CalmNullHandlerRegistationException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage = "The handler is null, so it could not be registerd.";

    /// <summary>
    /// The message with message type.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    /// <returns>the formatted message.</returns>
    private static string MessageWithType(Type messageType)
        => $"The handler for type {messageType.Name} is null, so it could not be registerd.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationException"/> class.
    /// </summary>
    public CalmNullHandlerRegistationException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmNullHandlerRegistationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmNullHandlerRegistationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationException"/> class.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    public CalmNullHandlerRegistationException(Type messageType)
        : base(messageType is null ? _defaultMessage : MessageWithType(messageType))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationException"/> class.
    /// </summary>
    /// <param name="messageType">The message Type of handler.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmNullHandlerRegistationException(Type messageType, Exception innerException)
        : base(messageType is null ? _defaultMessage : MessageWithType(messageType), innerException)
    {
    }
}
