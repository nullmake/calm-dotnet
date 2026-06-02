using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmNoHandlerRegisteredException"/> class.
/// </summary>
public class CalmNoHandlerRegisteredExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmNoHandlerRegisteredExceptionTests"/> class.
    /// </summary>
    public CalmNoHandlerRegisteredExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmNoHandlerRegisteredException();

        // Assert
        Assert.Equal("No handler registered.", exception.Message);
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
        var exception = new CalmNoHandlerRegisteredException(type);

        // Assert
        Assert.Equal($"No handler registered for type {type.Name}", exception.Message);
    }
}
