using Calm.Core.Tests.TestClasses.Messages;
using System.Diagnostics.CodeAnalysis;

namespace Calm.Core.Tests.TestClasses;

/// <summary>
/// Test class without generic handlers.
/// </summary>
internal sealed class TestGenericHandlerClass
{
    /// <summary>
    /// Gets and sets the wheather a generic method is called or not.
    /// </summary>
    public bool GenericMethodCalled { get; set; }

    /// <summary>
    /// Generic handler that should be skipped.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="parameter">The generic type parameter.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    [SuppressMessage("Usage", "CALM004:Invalid CalmHandler signature: Message type mismatch",
        Justification = "Test method for triggering an error.")]
    public Task HandleGenericAsync<T>(T parameter, CancellationToken token)
    {
        GenericMethodCalled = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Generic handler that should be skipped.
    /// </summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    /// <param name="parameter">The generic type parameter.</param>
    /// <param name="token">Optional user-provided cancellation token.</param>
    /// <returns>A task representing the response from the handler.</returns>
    [CalmHandler]
    [SuppressMessage("Usage", "CALM004:Invalid CalmHandler signature: Message type mismatch",
        Justification = "Test method for triggering an error.")]
    public Task<TestResponse> HandleGenericWithResponseAsync<T>(T parameter, CancellationToken token)
    {
        GenericMethodCalled = true;
        return Task.FromResult(new TestResponse());
    }
}
