using System.Windows.Input;
using System.Windows.Threading;
using ComputerLoadMonitoring.Services;

namespace ComputerLoadMonitoring.ViewModels;

public sealed class MainViewModel : ViewModelBase, IDisposable
{
    private readonly HardwareMonitorService _service;
    private readonly DispatcherTimer _timer;
    private bool _disposed;

    private float _cpuLoad;
    private float _cpuTemp;
    private float _gpuLoad;
    private float _gpuTemp;
    private float _ramUsage;
    private float _ramUsed;
    private float _ramTotal;
    private float _maxTemp;
    private bool _isClickThrough;
    private string _statusText = string.Empty;

    public float CpuLoad { get => _cpuLoad; private set => SetProperty(ref _cpuLoad, value); }
    public float CpuTemp { get => _cpuTemp; private set => SetProperty(ref _cpuTemp, value); }
    public float GpuLoad { get => _gpuLoad; private set => SetProperty(ref _gpuLoad, value); }
    public float GpuTemp { get => _gpuTemp; private set => SetProperty(ref _gpuTemp, value); }
    public float RamUsage { get => _ramUsage; private set => SetProperty(ref _ramUsage, value); }
    public float RamUsed { get => _ramUsed; private set => SetProperty(ref _ramUsed, value); }
    public float RamTotal { get => _ramTotal; private set => SetProperty(ref _ramTotal, value); }
    public float MaxTemp { get => _maxTemp; private set => SetProperty(ref _maxTemp, value); }
    public bool IsClickThrough { get => _isClickThrough; set => SetProperty(ref _isClickThrough, value); }
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }

    public ICommand ExitCommand { get; }
    public ICommand ToggleClickThroughCommand { get; }

    public MainViewModel()
    {
        _service = new HardwareMonitorService();

        ExitCommand = new RelayCommand(_ => System.Windows.Application.Current.Shutdown());
        ToggleClickThroughCommand = new RelayCommand(_ => IsClickThrough = !IsClickThrough);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += async (_, _) => await UpdateAsync();
        _timer.Start();

        _ = UpdateAsync();
    }

    private async Task UpdateAsync()
    {
        try
        {
            var data = await Task.Run(() => _service.GetHardwareData());
            CpuLoad = data.CpuLoad;
            CpuTemp = data.CpuTemp;
            GpuLoad = data.GpuLoad;
            GpuTemp = data.GpuTemp;
            RamUsage = data.RamUsage;
            RamUsed = data.RamUsed;
            RamTotal = data.RamTotal;
            MaxTemp = Math.Max(data.CpuTemp, data.GpuTemp);
            StatusText = string.Empty;
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _service.Dispose();
    }
}

internal sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
}
