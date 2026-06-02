#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the current thread is the designated engine thread.
/// </summary>
public class CalmAffinityException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage = "The current thread is the designated engine thread.";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmAffinityException"/> class.
    /// </summary>
    public CalmAffinityException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmAffinityException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmAffinityException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmAffinityException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmAffinityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
