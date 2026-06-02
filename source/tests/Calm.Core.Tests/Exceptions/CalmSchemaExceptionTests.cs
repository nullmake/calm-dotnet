using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmSchemaException"/> class.
/// </summary>
public class CalmSchemaExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmSchemaExceptionTests"/> class.
    /// </summary>
    public CalmSchemaExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmSchemaException();

        // Assert
        Assert.Equal("The system schema violates rules, such as method attributes,the presence or absence of generics, or interface implementations", exception.Message);
    }

    /// <summary>
    /// Verifies that the constructor with a message sets the message correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithMessageShouldSetMessage()
    {
        // Arrange
        const string message = "Invalid schema";

        // Act
        var exception = new CalmSchemaException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
