#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Exception thrown when the system schema violates rules, such as method attributes,
/// the presence or absence of generics, or interface implementations
/// </summary>
public class CalmSchemaException : CalmException
{
    /// <summary>
    /// The default message.
    /// </summary>
    private const string _defaultMessage =
        "The system schema violates rules, such as method attributes,"
        + "the presence or absence of generics, or interface implementations";

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmSchemaException"/> class.
    /// </summary>
    public CalmSchemaException()
        : base(_defaultMessage)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmSchemaException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public CalmSchemaException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CalmSchemaException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CalmSchemaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
