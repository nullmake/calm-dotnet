using Xunit;

namespace Calm.Core.Tests;

/// <summary>
/// Provides tests for <see cref="CalmOptions"/>.
/// </summary>
public class CalmOptionsTests
{
    /// <summary>
    /// Verifies that the default values are correctly initialized.
    /// </summary>
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        // Arrange & Act
        var options = new CalmOptions();

        // Assert
        Assert.Equal(10000, options.Capacity);
        Assert.Null(options.ErrorObserver);
        Assert.Equal(TimeProvider.System, options.TimeProvider);
        Assert.Equal(TimeSpan.FromSeconds(5), options.WatchdogThreshold);
        Assert.True(options.EnableLogger);
    }

    /// <summary>
    /// Verifies that properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void PropertiesCanBeSet()
    {
        // Arrange
        var mockObserver = new Moq.Mock<ICalmErrorObserver>().Object;
        var mockTimeProvider = new Moq.Mock<TimeProvider>().Object;

        // Act
        var options = new CalmOptions
        {
            Capacity = 500,
            ErrorObserver = mockObserver,
            TimeProvider = mockTimeProvider,
            WatchdogThreshold = TimeSpan.FromSeconds(10),
            EnableLogger = false
        };

        // Assert
        Assert.Equal(500, options.Capacity);
        Assert.Same(mockObserver, options.ErrorObserver);
        Assert.Same(mockTimeProvider, options.TimeProvider);
        Assert.Equal(TimeSpan.FromSeconds(10), options.WatchdogThreshold);
        Assert.False(options.EnableLogger);
    }

    /// <summary>
    /// Verifies that ToString() returns the expected format.
    /// </summary>
    [Fact]
    public void ToStringReturnsExpectedFormat()
    {
        // Arrange
        var options = new CalmOptions
        {
            Capacity = 100,
            WatchdogThreshold = TimeSpan.FromSeconds(1.234)
        };

        // Act
        var result = options.ToString();

        // Assert
        // Expected format: { Capacity=100, ErrorObserver=null, TimeProvider=SystemTimeProvider, WatchdogThreshold=1.234s, EnableLogger=True }
        // Note: TimeProvider.System.GetType().Name might vary by platform but usually ends with TimeProvider.
        Assert.DoesNotContain("Mode=", result, StringComparison.Ordinal);
        Assert.Contains("Capacity=100", result, StringComparison.Ordinal);
        Assert.Contains("ErrorObserver=null", result, StringComparison.Ordinal);
        Assert.Contains("WatchdogThreshold=1.234s", result, StringComparison.Ordinal);
        Assert.Contains("EnableLogger=True", result, StringComparison.Ordinal);
    }
}
