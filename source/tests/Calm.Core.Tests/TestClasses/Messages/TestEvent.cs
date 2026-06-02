namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// Represents a test event used for integration testing scenarios.
/// </summary>
/// <param name="message">The message.</param>
/// <param name="funcAsync">The asynchronous processing.</param>
internal sealed class TestEvent(string message, Func<object, CancellationToken, Task>? funcAsync = null)
    : ICalmEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestQuery"/> class.
    /// </summary>
    public TestEvent()
        : this("", null)
    {
    }

    /// <summary>
    /// Gets or sets the event message.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Gets or sets the asynchronous processing.
    /// </summary>
    public Func<object, CancellationToken, Task> FuncAsync { get; set; }
        = funcAsync ?? ((_, _) => Task.CompletedTask);

    /// <inheritdoc/>
    public override string ToString()
        => $"{{ Message=\"{Message}\", FuncAsync={funcAsync?.Method.Name ?? "null"} }}";
}
