using Calm.Core;
using Calm.Extensions.DependencyInjection.Tests.TestClasses;
using Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Calm.Extensions.DependencyInjection.Tests;

/// <summary>
/// Provides tests for Dependency Injection integration.
/// </summary>
public class DependencyInjectionTests() : TestBase(LogLevel.Trace)
{
    /// <summary>
    /// Verifies that engines and handlers are correctly registered and connected via DI.
    /// </summary>
    /// <param name="classType">The type of the class containing methods marked
    /// with <see cref="CalmHandlerAttribute"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData(typeof(DiTestClass))]
    public async Task DiSetupShouldConnectHandlersAndProcessMessages(Type? classType)
    {
        var TestCtxCT = TestContext.Current.CancellationToken;

        // Arrange
        var hostBuilder = CreateTestBuilder()
            .ConfigureServices(services =>
            {
                services.AddCalm();
                if (classType is null)
                {
                    var assembly = typeof(DependencyInjectionTests).Assembly;
                    services.AddCalmHandlersFromAssembly(ServiceLifetime.Transient, assembly,
                        info => !(info.DeclaringType == typeof(DiAttributeClass)
                            && info.ParameterType == typeof(TestQuery)));
                }
                else
                {
                    services.AddCalmHandlersFromClass(ServiceLifetime.Transient, classType);
                }
            });

        using var host = hostBuilder.Build();

        // Act: Start the host (triggers CalmConnectionService)
        await host.StartAsync(CancellationToken.None);

        var calm = host.Services.GetRequiredService<ICalm>();
        var handler = host.Services.GetRequiredService<DiTestClass>();

        // Test Request/Response through DI-wired setup
        var response = await calm.Query.SendAsync(new TestQuery("DI Test"), TestCtxCT);

        // Assert
        Assert.Equal("DI: DI Test", response);
        Assert.True(handler.WasCalled);

        // Cleanup
        await host.StopAsync(CancellationToken.None);
    }
}
