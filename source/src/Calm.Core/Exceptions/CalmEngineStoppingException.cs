#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// An exception thrown when the engine is stopped and cannot accept new tasks.
/// </summary>
public class CalmEngineStoppingException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage = "The Calm engine is stopping and cannot accept new external tasks.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngineStoppingException"/> class.
    /// </summary>
    public CalmEngineStoppingException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngineStoppingException"/> class.
    /// </summary>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmEngineStoppingException(Exception innerException)
        : base(_defaultMessage, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngineStoppingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmEngineStoppingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngineStoppingException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmEngineStoppingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
