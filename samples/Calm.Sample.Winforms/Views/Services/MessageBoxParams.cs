using System.Diagnostics.CodeAnalysis;

namespace Calm.Sample.Winforms.Views.Services;

/// <summary>
/// MessageBox parameters.
/// </summary>
internal struct MessageBoxParams : IEquatable<MessageBoxParams>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBoxParams"/> struct.
    /// </summary>
    public MessageBoxParams()
    {
    }

    /// <summary>
    /// An implementation of IWin32Window that will own the modal dialog box.
    /// </summary>
    public IWin32Window? Owner { get; set; }

    /// <summary>
    /// The text to display in the message box.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// The text to display in the title bar of the message box.
    /// </summary>
    public string? Caption { get; set; } = "";

    /// <summary>
    /// One of the <see cref="MessageBoxButtons"/> values that specifies
    /// which buttons to display in the message box.
    /// </summary>
    public MessageBoxButtons Buttons { get; set; } = MessageBoxButtons.OK;

    /// <summary>
    /// One of the <see cref="MessageBoxIcon"/> values that specifies
    /// which icon to display in the message box.
    /// </summary>
    public MessageBoxIcon Icon { get; set; } = MessageBoxIcon.None;

    /// <summary>
    /// One of the <see cref="MessageBoxDefaultButton"/> values
    /// that specifies the default button for the message box.
    /// </summary>
    public MessageBoxDefaultButton DefaultButton { get; set; } = MessageBoxDefaultButton.Button1;

    /// <summary>
    /// One of the <see cref="MessageBoxOptions"/> values that specifies
    /// which display and association options will be used for the message box.
    /// You may pass in 0 if you wish to use the defaults.
    /// </summary>
    public MessageBoxOptions Options { get; set; }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
        => obj is MessageBoxParams @params && Equals(@params);

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public readonly bool Equals(MessageBoxParams other)
        => EqualityComparer<IWin32Window?>.Default.Equals(Owner, other.Owner)
            && string.Equals(Text, other.Text, StringComparison.Ordinal)
            && string.Equals(Caption, other.Caption, StringComparison.Ordinal)
            && Buttons == other.Buttons
            && Icon == other.Icon
            && DefaultButton == other.DefaultButton
            && Options == other.Options;

    /// <inheritdoc/>
    public override readonly int GetHashCode()
        => HashCode.Combine(Owner, Text, Caption, Buttons, Icon, DefaultButton, Options);
}
