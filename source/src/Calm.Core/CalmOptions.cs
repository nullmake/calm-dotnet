using System.Globalization;
using System.Text;

namespace Calm.Core;

/// <summary>
/// Provides configuration options for <see cref="CalmEngine"/>.
/// </summary>
public sealed class CalmOptions : ICalmOptions
{
    /// <inheritdoc/>
    public int Capacity { get; set; } = 10000;

    /// <summary>
    /// Gets an optional observer for unhandled exceptions.
    /// </summary>
    public ICalmErrorObserver? ErrorObserver { get; set; }

    /// <inheritdoc/>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <inheritdoc/>
    public TimeSpan WatchdogThreshold { get; set; } = TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public bool EnableLogger { get; set; } = true;

    /// <inheritdoc/>
    public override string ToString()
        => new StringBuilder()
            .Append("{ ").Append("Capacity=").Append(Capacity)
            .Append(", ").Append("ErrorObserver=").Append(ErrorObserver?.GetType().Name ?? "null")
            .Append(", ").Append("TimeProvider=").Append(TimeProvider.GetType().Name)
            .Append(", ").Append("WatchdogThreshold=").Append(
                WatchdogThreshold.TotalSeconds.ToString("0.000s", CultureInfo.InvariantCulture))
            .Append(", ").Append("EnableLogger=").Append(EnableLogger)
            .Append(" }")
            .ToString();
}
