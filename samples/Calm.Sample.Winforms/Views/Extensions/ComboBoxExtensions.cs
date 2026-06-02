using System.Globalization;
using System.Reflection;

namespace Calm.Sample.Winforms.Views.Extensions;

/// <summary>
/// The extensions for <see cref="ComboBoxExtensions"/>.
/// </summary>
internal static class ComboBoxExtensions
{
    /// <summary>
    /// Set the value to the <see langword="DataSource"/> property.
    /// </summary>
    /// <typeparam name="T">The type of the data source.</typeparam>
    /// <param name="comboBox">The <see cref="ComboBox"/> instance.</param>
    /// <param name="listItemData">The data source to be set to the <see langword="DataSource"/> property.</param>
    public static void SetDataSource<T>(this ComboBox comboBox, IEnumerable<T> listItemData)
    {
        comboBox.DataSource = listItemData;
        comboBox.DisplayMember = "Display";
        comboBox.ValueMember = "Value";
        comboBox.AdjustWidth();
    }

    /// <summary>
    /// Gets the value of the member property specified by the ValueMember property.
    /// </summary>
    /// <typeparam name="T">The type of the ValueMember property.</typeparam>
    /// <param name="comboBox">The <see cref="ComboBox"/> instance.</param>
    /// <returns>The ValueMember property.</returns>
    public static T SelectedValue<T>(this ComboBox comboBox)
        => (T)Convert.ChangeType(comboBox.SelectedValue, typeof(T), CultureInfo.InvariantCulture)!;

    /// <summary>
    /// Resize the drop-down list to fit the length of its items.
    /// </summary>
    /// <param name="comboBox">The <see cref="ComboBox"/> instance.</param>
    public static void AdjustWidth(this ComboBox comboBox)
    {
        var maxWidth = 0;
        var valueGetter = GetValueGetter(comboBox);

        using (Graphics g = comboBox.CreateGraphics())
        {
            foreach (var item in comboBox.Items)
            {

                int itemWidth = (int)g.MeasureString(valueGetter(item), comboBox.Font).Width;
                itemWidth += SystemInformation.VerticalScrollBarWidth;
                if (maxWidth < itemWidth)
                {
                    maxWidth = itemWidth;
                }
            }
        }
        if (maxWidth > 0)
        {
            comboBox.Width = maxWidth;
        }
    }

    /// <summary>
    /// Gets a <see cref="ComboBox.Items"/> value getter.
    /// </summary>
    /// <param name="comboBox">The <see cref="ComboBox"/> instance.</param>
    /// <returns>The <see cref="ComboBox.Items"/> value getter.</returns>
    private static Func<object, string> GetValueGetter(ComboBox comboBox)
    {
        var displayMember = comboBox.DisplayMember;
        if (string.IsNullOrWhiteSpace(displayMember))
        {
            static string ValueGetter(object obj)
            {
                return string.Format(CultureInfo.CurrentCulture, "{0}", obj);
            }
            return ValueGetter;
        }
        else
        {
            string ValueGetter(object obj)
            {
                var memberInfos = obj.GetType().GetMember(displayMember);
                if (memberInfos.Length > 0)
                {
                    if (memberInfos[0] is PropertyInfo propertyInfo)
                    {
                        return string.Format(CultureInfo.CurrentCulture, "{0}", propertyInfo.GetValue(obj));
                    }
                    if (memberInfos[0] is FieldInfo fieldInfo)
                    {
                        return string.Format(CultureInfo.CurrentCulture, "{0}", fieldInfo.GetValue(obj));
                    }
                }
                return "";
            }
            return ValueGetter;
        }
    }
}
