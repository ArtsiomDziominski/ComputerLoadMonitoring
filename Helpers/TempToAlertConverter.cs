using System.Globalization;
using System.Windows.Data;

namespace ComputerLoadMonitoring.Helpers;

[ValueConversion(typeof(float), typeof(bool))]
internal sealed class TempToAlertConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is float temp && temp >= 90;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
