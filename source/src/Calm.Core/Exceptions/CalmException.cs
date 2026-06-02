#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents errors that occur during Calm execution.
/// </summary>
public class CalmException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmException"/> class.
    /// </summary>
    public CalmException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
