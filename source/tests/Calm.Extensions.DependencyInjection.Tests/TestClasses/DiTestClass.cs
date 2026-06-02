using Calm.Core;
using Calm.Extensions.DependencyInjection.Tests.TestClasses.Messages;

namespace Calm.Extensions.DependencyInjection.Tests.TestClasses;

/// <summary>
/// A test handler for DI integration.
/// </summary>
internal sealed class DiTestClass
{
    /// <summary>
    /// Gets a value indicating whether the handler was called.
    /// </summary>
    public bool WasCalled { get; private set; }

    /// <summary>
    /// Handles the DiRequest asynchronously.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="token">Optional cancellation token for the user operation.</param>
    /// <returns>A task containing the response string.</returns>
    /// <exception cref="ArgumentNullException">The request parameter is null.</exception>
    [CalmHandler]
    public Task<string> HandleTestQueryAsync(TestQuery request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);

        WasCalled = true;
        return Task.FromResult($"DI: {request.Data}");
    }
}
