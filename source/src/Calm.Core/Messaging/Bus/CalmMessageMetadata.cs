using System.Collections.Concurrent;

namespace Calm.Core.Messaging.Bus;

/// <summary>
/// Provides cached metadata for message types to optimize behavioral lookups.
/// </summary>
internal sealed class CalmMessageMetadata
{
    /// <summary>
    /// The cache for message type metadata.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, CalmMessageMetadata> _cache = new();

    /// <summary>
    /// Gets the default metadata used when no attributes are present.
    /// </summary>
    public static CalmMessageMetadata Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the message should bypass the outbox (Events only).
    /// </summary>
    public bool Immediate { get; init; }

    /// <summary>
    /// Gets a value indicating whether logging should be suppressed for this message.
    /// </summary>
    public bool SuppressLog { get; init; }

    /// <summary>
    /// Retrieves or creates the metadata for the specified message type.
    /// </summary>
    /// <param name="type">The type of the message.</param>
    /// <returns>The metadata for the message type.</returns>
    public static CalmMessageMetadata Get(Type type)
    {
        return _cache.GetOrAdd(type, t =>
        {
            var immediate = Attribute.IsDefined(t, typeof(CalmImmediateAttribute));
            var suppressLog = Attribute.IsDefined(t, typeof(CalmSuppressLogAttribute));

            if (!immediate && !suppressLog)
            {
                return Default;
            }

            return new CalmMessageMetadata
            {
                Immediate = immediate,
                SuppressLog = suppressLog
            };
        });
    }
}
