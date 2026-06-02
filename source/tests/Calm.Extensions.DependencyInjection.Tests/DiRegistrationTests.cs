using Calm.Core;
using Calm.Extensions.DependencyInjection.Tests.TestClasses;
using Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Calm.Extensions.DependencyInjection.Tests;

/// <summary>
/// Provides tests for DI registration with [CalmHandler] attribute pattern.
/// </summary>
public class DiRegistrationTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that AddCalmHandler registers handlers correctly.
    /// </summary>
    [Fact]
    public void AddCalmHandlerShouldRegisterHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCalmHandlersFromClass<DiAttributeClass>(ServiceLifetime.Transient);

        // Assert
        var handlerDescriptors = services.Where(s => s.ServiceType == typeof(DiAttributeClass)).ToList();
        Assert.Single(handlerDescriptors);
    }

    /// <summary>
    /// Verifies that AddCalmHandler registers handlers with correct lifetime.
    /// </summary>
    /// <param name="lifetime">The service lifetime to test.</param>
    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCalmHandlerShouldRegisterWithCorrectLifetime(ServiceLifetime lifetime)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCalmHandlersFromClass<DiAttributeClass>(lifetime);

        // Assert
        var descriptors = services.Where(s => s.ServiceType == typeof(DiAttributeClass)).ToList();
        Assert.Single(descriptors);
        Assert.Equal(lifetime, descriptors[0].Lifetime);
    }

    /// <summary>
    /// Verifies that multiple handlers can be registered.
    /// </summary>
    [Fact]
    public void AddCalmHandlerShouldRegisterMultipleHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTransientCalmHandlersFromClass<DiAttributeClass>();
        services.AddTransientCalmHandlersFromClass<DiTestClass>();

        // Assert
        var attributeDescriptors = services.Where(s => s.ServiceType == typeof(DiAttributeClass)).ToList();
        var testDescriptors = services.Where(s => s.ServiceType == typeof(DiTestClass)).ToList();
        Assert.Single(attributeDescriptors);
        Assert.Single(testDescriptors);
    }

    /// <summary>
    /// Verifies that hosted service automatically connects handlers on startup.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Test method")]
    public async Task HostedServiceShouldAutomaticallyConnectHandlersOnStartup()
    {
        // Arrange
        var hostBuilder = CreateTestBuilder()
            .ConfigureServices(services =>
            {
                services.AddCalm();
                services.AddSingletonCalmHandlersFromClass<DiAttributeClass>();
            });

        using var host = hostBuilder.Build();

        // Act: Start the host
        await host.StartAsync(CancellationToken.None);

        var calm = host.Services.GetRequiredService<ICalm>();
        var handler = host.Services.GetRequiredService<DiAttributeClass>();

        // Send a test message to verify handler is connected
        await calm.Command.SendAsync(new TestCommand("Test"), CancellationToken.None);

        // Assert
        Assert.True(handler.CommandWasCalled, "Command handler should be called");

        // Cleanup
        await host.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// Verifies that hosted service disconnects handlers on shutdown.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Test method")]
    public async Task HostedServiceShouldDisconnectHandlersOnShutdown()
    {
        // Arrange
        var hostBuilder = CreateTestBuilder()
            .ConfigureServices(services =>
            {
                services.AddCalm();
                services.AddTransientCalmHandlersFromClass<DiAttributeClass>();
            });

        using var host = hostBuilder.Build();
        await host.StartAsync(CancellationToken.None);

        var calm = host.Services.GetRequiredService<ICalm>();
        var handler = host.Services.GetRequiredService<DiAttributeClass>();

        // Verify handlers are connected by sending a message
        await calm.Command.SendAsync(new TestCommand("Test"), CancellationToken.None);
        Assert.True(handler.CommandWasCalled);

        // Act: Stop the host
        await host.StopAsync(CancellationToken.None);

        // Assert: Engine should be stopped
    }

    /// <summary>
    /// Verifies that message handling works via DI.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Test method")]
    public async Task MessageHandlingShouldWorkViaDi()
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var hostBuilder = CreateTestBuilder()
            .ConfigureServices(services =>
            {
                services.AddCalm();
                services.AddSingletonCalmHandlersFromClass<DiAttributeClass>();
            });

        using var host = hostBuilder.Build();
        await host.StartAsync(CancellationToken.None);

        var calm = host.Services.GetRequiredService<ICalm>();
        var handler = host.Services.GetRequiredService<DiAttributeClass>();

        // Act: Send command
        await calm.Command.SendAsync(new TestCommand("Test Command"), CancellationToken.None);

        // Act: Send query
        var queryResult = await calm.Query.SendAsync(new TestQuery("Test Query"), CancellationToken.None);

        // Act: Publish event (Note: PublishAsync returns void, so we need to wait for processing)
        calm.Event.Publish(new TestEvent("Test Event"), CancellationToken.None);

        // Wait for event to be processed
        await WaitForIdleAsync(calm, TestCtxCT);

        // Assert
        Assert.True(handler.CommandWasCalled, "CommandWasCalled");
        Assert.Equal("Test Command", handler.LastCommandData);
        Assert.Equal("Test Query", handler.LastQueryData);
        Assert.Equal("Attribute: Test Query", queryResult);
        Assert.True(handler.EventWasCalled, "EventWasCalled");
        Assert.Equal("Test Event", handler.LastEventData);

        // Cleanup
        await host.StopAsync(CancellationToken.None);
    }
}
