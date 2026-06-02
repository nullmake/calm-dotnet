using SharedLibrary;
using System.Globalization;
using System.Windows.Data;

namespace Sample04.Views.Converters;

internal sealed class HumanReadableByteSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || !double.TryParse(value.ToString(), out double bytes))
        {
            return "";
        }
        return bytes.ToHumanReadableByteSize();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
