using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmNullHandlerRegistationException"/> class.
/// </summary>
public class CalmNullHandlerRegistationExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNullHandlerRegistationExceptionTests"/> class.
    /// </summary>
    public CalmNullHandlerRegistationExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmNullHandlerRegistationException();

        // Assert
        Assert.Equal("The handler is null, so it could not be registerd.", exception.Message);
    }

    /// <summary>
    /// Verifies that the constructor with a type sets the message correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithTypeShouldSetFormattedMessage()
    {
        // Arrange
        var type = typeof(string);

        // Act
        var exception = new CalmNullHandlerRegistationException(type);

        // Assert
        Assert.Equal($"The handler for type {type.Name} is null, so it could not be registerd.", exception.Message);
    }
}
