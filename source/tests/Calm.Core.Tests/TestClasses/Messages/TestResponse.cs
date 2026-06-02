namespace Calm.Core.Tests.TestClasses.Messages;

/// <summary>
/// Test response.
/// </summary>
/// <param name="output">The output value.</param>
internal sealed class TestResponse(string output = "")
{
    /// <summary>
    /// Gets or sets the output.
    /// </summary>
    public string Output { get; set; } = output;

    /// <inheritdoc/>
    public override string ToString()
        => $"{{ Output={Output} }}";
}
