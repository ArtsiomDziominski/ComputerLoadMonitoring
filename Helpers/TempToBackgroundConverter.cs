using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ComputerLoadMonitoring.Helpers;

[ValueConversion(typeof(float), typeof(SolidColorBrush))]
internal sealed class TempToBackgroundConverter : IValueConverter
{
    private static readonly Color NormalColor = Color.FromArgb(0xCC, 0x1E, 0x1E, 0x1E);
    private static readonly Color HotColor = Color.FromArgb(0xCC, 0xB7, 0x1C, 0x1C);
    private static readonly Color CriticalColor = Color.FromArgb(0xCC, 0xD3, 0x2F, 0x2F);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var temp = value is float f ? f : 0f;

        if (temp < 70)
            return new SolidColorBrush(NormalColor);

        if (temp >= 80)
            return new SolidColorBrush(CriticalColor);

        var t = (temp - 70f) / 10f;
        var r = Lerp(NormalColor.R, HotColor.R, t);
        var g = Lerp(NormalColor.G, HotColor.G, t);
        var b = Lerp(NormalColor.B, HotColor.B, t);

        return new SolidColorBrush(Color.FromArgb(0xCC, r, g, b));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static byte Lerp(byte a, byte b, float t)
        => (byte)(a + (b - a) * t);
}
