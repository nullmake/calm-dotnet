using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Core.Tests.Exceptions;

/// <summary>
/// Provides tests for the <see cref="CalmHandlerAlreadyRegisteredException"/> class.
/// </summary>
public class CalmHandlerAlreadyRegisteredExceptionTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CalmHandlerAlreadyRegisteredExceptionTests"/> class.
    /// </summary>
    public CalmHandlerAlreadyRegisteredExceptionTests() : base(LogLevel.Trace)
    {
    }

    /// <summary>
    /// Verifies that the default constructor sets the default message.
    /// </summary>
    [Fact]
    public void ConstructorDefaultShouldSetDefaultMessage()
    {
        // Act
        var exception = new CalmHandlerAlreadyRegisteredException();

        // Assert
        Assert.Equal("A handler is already registered.", exception.Message);
    }

    /// <summary>
    /// Verifies that the constructor with type information sets the message correctly.
    /// </summary>
    [Fact]
    public void ConstructorWithTypeInfoShouldSetFormattedMessage()
    {
        // Arrange
        var type = typeof(string);
        const string adding = "NewMethod";
        const string existing = "OldMethod";

        // Act
        var exception = new CalmHandlerAlreadyRegisteredException(type, adding, existing);

        // Assert
        Assert.Contains(type.Name, exception.Message, StringComparison.Ordinal);
        Assert.Contains(adding, exception.Message, StringComparison.Ordinal);
        Assert.Contains(existing, exception.Message, StringComparison.Ordinal);
    }
}
