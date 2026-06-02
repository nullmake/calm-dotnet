namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// Command for testing.
/// </summary>
/// <param name="input">The request value.</param>
/// <param name="funcAsync">The asynchronous processing.</param>
internal sealed class TestCommand(string input, Func<object, CancellationToken, Task>? funcAsync = null)
    : ICalmCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestQuery"/> class.
    /// </summary>
    public TestCommand()
        : this("", null)
    {
    }

    /// <summary>
    /// Gets or sets the input.
    /// </summary>
    public string Input { get; set; } = input;

    /// <summary>
    /// Gets or sets the asynchronous processing.
    /// </summary>
    public Func<object, CancellationToken, Task> FuncAsync { get; set; }
        = funcAsync ?? ((_, _) => Task.CompletedTask);

    /// <inheritdoc/>
    public override string ToString()
        => $"{{ Input=\"{Input}\", FuncAsync={funcAsync?.Method.Name ?? "null"} }}";
}
