using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmEngineStoppingException"/> class.
/// </summary>
public class CalmEngineStoppingExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmEngineStoppingExceptionTests"/> class.
    /// </summary>
    public CalmEngineStoppingExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmEngineStoppingException();

        // Assert
        Assert.Equal("The Calm engine is stopping and cannot accept new external tasks.", exception.Message);
    }

    /// <summary>
    /// Verifies that the constructor with an inner exception sets the message and inner exception correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithInnerExceptionShouldSetMessageAndInner()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner exception");

        // Act
        var exception = new CalmEngineStoppingException(inner);

        // Assert
        Assert.Equal("The Calm engine is stopping and cannot accept new external tasks.", exception.Message);
        Assert.Same(inner, exception.InnerException);
    }

    /// <summary>
    /// Verifies that the constructor with a message sets the message correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithMessageShouldSetMessage()
    {
        // Arrange
        const string message = "Custom error message";

        // Act
        var exception = new CalmEngineStoppingException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
