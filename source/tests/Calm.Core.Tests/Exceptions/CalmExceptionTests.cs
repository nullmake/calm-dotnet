using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmException"/> class.
/// </summary>
public class CalmExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmExceptionTests"/> class.
    /// </summary>
    public CalmExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor works.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldWork()
    {
        // Act
        var exception = new CalmException();

        // Assert
        Assert.NotNull(exception);
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
        var exception = new CalmException(message);

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
        var exception = new CalmException(message, inner);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(inner, exception.InnerException);
    }
}
