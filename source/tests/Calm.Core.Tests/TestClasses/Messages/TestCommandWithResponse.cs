namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// Command for testing.
/// </summary>
/// <param name="input">The request value.</param>
/// <param name="output">The response value.</param>
/// <param name="funcAsync">The asynchronous processing.</param>
internal sealed class TestCommandWithResponse(
    string input, string output, Func<object, CancellationToken, Task>? funcAsync = null)
    : ICalmCommand<TestResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestQuery"/> class.
    /// </summary>
    public TestCommandWithResponse()
        : this("", "", null)
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

    /// <summary>
    /// Gets or sets the output value;
    /// </summary>
    public string Output { get; set; } = output;

    /// <inheritdoc/>
    public override string ToString()
        => $"{{ Input=\"{Input}\", Output=\"{Output}\", FuncAsync={funcAsync?.Method.Name ?? "null"} }}";
}
