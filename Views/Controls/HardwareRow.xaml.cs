using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ComputerLoadMonitoring.Views.Controls;

public partial class HardwareRow : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(HardwareRow));

    public static readonly DependencyProperty LoadValueProperty =
        DependencyProperty.Register(nameof(LoadValue), typeof(string), typeof(HardwareRow));

    public static readonly DependencyProperty LoadColorProperty =
        DependencyProperty.Register(nameof(LoadColor), typeof(Brush), typeof(HardwareRow),
            new PropertyMetadata(Brushes.White));

    public static readonly DependencyProperty TempValueProperty =
        DependencyProperty.Register(nameof(TempValue), typeof(string), typeof(HardwareRow));

    public static readonly DependencyProperty TempColorProperty =
        DependencyProperty.Register(nameof(TempColor), typeof(Brush), typeof(HardwareRow),
            new PropertyMetadata(Brushes.White));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string LoadValue
    {
        get => (string)GetValue(LoadValueProperty);
        set => SetValue(LoadValueProperty, value);
    }

    public Brush LoadColor
    {
        get => (Brush)GetValue(LoadColorProperty);
        set => SetValue(LoadColorProperty, value);
    }

    public string TempValue
    {
        get => (string)GetValue(TempValueProperty);
        set => SetValue(TempValueProperty, value);
    }

    public Brush TempColor
    {
        get => (Brush)GetValue(TempColorProperty);
        set => SetValue(TempColorProperty, value);
    }

    public HardwareRow()
    {
        InitializeComponent();
    }
}
