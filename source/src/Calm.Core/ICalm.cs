using Calm.Core.Engines;
using Calm.Core.Messaging;

namespace Calm.Core;

/// <summary>
/// A unified interface that combines both engine execution and message bus capabilities.
/// This serves as the primary entry point for users of the CALM library.
/// </summary>
public interface ICalm : ICalmPump, ICalmBus
{
    /// <summary>
    /// The configuration options for <see cref="CalmEngine"/>.
    /// </summary>
    ICalmOptions Options { get; }
}
