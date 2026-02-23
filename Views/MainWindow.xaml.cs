using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ComputerLoadMonitoring.Helpers;
using ComputerLoadMonitoring.Services;
using ComputerLoadMonitoring.ViewModels;

namespace ComputerLoadMonitoring.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RestoreSettings();
        Loaded += OnLoaded;
    }

    private void RestoreSettings()
    {
        var s = WindowSettingsService.Load();
        Width = s.Width;

        if (!double.IsNaN(s.Left) && !double.IsNaN(s.Top))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = s.Left;
            Top = s.Top;
        }

        UpdateWidthMenuChecks(s.Width);
        StartupMenuItem.IsChecked = StartupService.IsEnabled;
    }

    private void UpdateWidthMenuChecks(double width)
    {
        if (ContextMenu == null) return;
        var widthTag = ((int)width).ToString();

        foreach (var item in ContextMenu.Items)
        {
            if (item is System.Windows.Controls.MenuItem { Header: "Width" } widthMenu)
            {
                foreach (var sub in widthMenu.Items)
                {
                    if (sub is System.Windows.Controls.MenuItem mi)
                        mi.IsChecked = mi.Tag is string t && t == widthTag;
                }
                break;
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        HideFromAltTab();
    }

    private void HideFromAltTab()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
            exStyle | NativeMethods.WS_EX_TOOLWINDOW);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.IsClickThrough))
            return;

        var vm = (MainViewModel)sender!;
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);

        if (vm.IsClickThrough)
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
                exStyle | NativeMethods.WS_EX_TRANSPARENT);
        else
            NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
                exStyle & ~NativeMethods.WS_EX_TRANSPARENT);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void StartupMenuItem_Click(object sender, RoutedEventArgs e)
    {
        StartupService.SetEnabled(StartupMenuItem.IsChecked);
    }

    private void WidthOption_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem clicked || clicked.Tag is not string tag)
            return;

        if (!double.TryParse(tag, out var newWidth))
            return;

        Width = newWidth;

        var parent = clicked.Parent as System.Windows.Controls.ItemsControl;
        if (parent == null) return;

        foreach (var item in parent.Items)
        {
            if (item is System.Windows.Controls.MenuItem mi)
                mi.IsChecked = mi == clicked;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        WindowSettingsService.Save(new WindowSettings
        {
            Left = Left,
            Top = Top,
            Width = Width
        });

        if (DataContext is MainViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_PropertyChanged;
            vm.Dispose();
        }
        base.OnClosing(e);
    }
}
