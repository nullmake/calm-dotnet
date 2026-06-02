using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmAffinityException"/> class.
/// </summary>
public class CalmAffinityExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmAffinityExceptionTests"/> class.
    /// </summary>
    public CalmAffinityExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmAffinityException();

        // Assert
        Assert.Equal("The current thread is the designated engine thread.", exception.Message);
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
        var exception = new CalmAffinityException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    /// <summary>
    /// Verifies that the constructor with a message and inner exception sets them correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithMessageAndInnerExceptionShouldSetBoth()
    {
        // Arrange
        const string message = "Custom error message";
        var inner = new InvalidOperationException("Inner exception");

        // Act
        var exception = new CalmAffinityException(message, inner);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(inner, exception.InnerException);
    }
}
