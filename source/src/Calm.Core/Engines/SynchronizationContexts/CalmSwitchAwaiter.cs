using Calm.Core.Engines;
using Calm.Core.Engines.SynchronizationContexts;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Calm.Core;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// A custom awaiter to support the 'await engine.SwitchAsync()' pattern,
/// enabling seamless context switching to the engine's execution thread.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CalmSwitchAwaiter"/> struct.
/// </remarks>
/// <param name="pump">The target message pump.</param>
public readonly struct CalmSwitchAwaiter(ICalmPump pump) : INotifyCompletion, IEquatable<CalmSwitchAwaiter>
{
    /// <summary>
    /// The engine instance to switch to.
    /// </summary>
    private readonly ICalmPump _pump = pump;

    /// <summary>
    /// Gets a value indicating whether the switch is already complete.
    /// Returns <see langword="true"/> if the caller is already on the target engine thread.
    /// </summary>
    public bool IsCompleted => _pump.IsOnEngineThread;

    /// <summary>
    /// Schedules the continuation onto the engine's execution thread.
    /// </summary>
    /// <param name="continuation">The action to invoke on the engine thread.</param>
    public void OnCompleted(Action continuation)
    {
        if (_pump is ICalmSynchronizationContextDispatcher dispatcher)
        {
            // Pass the continuation directly as state to avoid closure and dummy object allocations.
            dispatcher.Post(static s => ((Action)s!)(), continuation);
        }
        else
        {
            // Fallback to Schedule if the pump doesn't support direct dispatching.
            // This should not happen with the standard CalmPump implementation.
            _pump.Schedule(_ =>
            {
                continuation();
                return Task.CompletedTask;
            }, CancellationToken.None, "Continuation");
        }
    }

    /// <summary>
    /// Ends the await operation. This is called by the compiler-generated state machine.
    /// </summary>
    public void GetResult()
    {
        // Do nothing - the purpose of this method is simply to satisfy the awaiter pattern.
    }

    /// <summary>
    /// Returns this awaiter instance to satisfy the awaitable pattern.
    /// </summary>
    /// <returns>The current <see cref="CalmSwitchAwaiter"/> instance.</returns>
    public CalmSwitchAwaiter GetAwaiter() => this;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is CalmSwitchAwaiter other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(CalmSwitchAwaiter other) => _pump == other._pump;

    /// <inheritdoc/>
    public override int GetHashCode() => _pump?.GetHashCode() ?? 0;

    /// <summary>
    /// Compares two <see cref="CalmSwitchAwaiter"/> instances for equality.
    /// </summary>
    /// <param name="left">The first CalmSwitchAwaiter instance to compare.</param>
    /// <param name="right">The second CalmSwitchAwaiter instance to compare.</param>
    /// <returns>true if the specified CalmSwitchAwaiter instances are equal; otherwise, false.</returns>
    public static bool operator ==(CalmSwitchAwaiter left, CalmSwitchAwaiter right)
        => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="CalmSwitchAwaiter"/> instances for inequality.
    /// </summary>
    /// <param name="left">The first CalmSwitchAwaiter instance to compare.</param>
    /// <param name="right">The second CalmSwitchAwaiter instance to compare.</param>
    /// <returns>true if the specified CalmSwitchAwaiter instances are not equal; otherwise, false.</returns>
    public static bool operator !=(CalmSwitchAwaiter left, CalmSwitchAwaiter right)
        => !left.Equals(right);
}
