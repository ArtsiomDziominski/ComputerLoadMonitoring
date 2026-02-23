# ComputerLoadMonitoring

A lightweight Windows desktop widget that displays real-time system metrics:

- CPU load (%) and temperature (°C)
- GPU load (%) and temperature (°C)
- RAM usage (%)

Built with WPF (.NET 10) and [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 or later
- Administrator privileges (required for hardware sensor access)

## Build & Run

```bash
# Restore NuGet packages
dotnet restore

# Build
dotnet build --configuration Release

# Run (must be run as Administrator)
dotnet run --configuration Release
```

Or press **F5** in VS Code / Cursor (run the IDE as Administrator).

## Publish (single-file executable)

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -o publish
```

The result is a single `publish/ComputerLoadMonitoring.exe` (~30 MB) that includes the .NET runtime and all dependencies — no SDK installation required on the target machine. Run it as Administrator.

## Usage

- **Drag** the widget by clicking and holding anywhere on it.
- **Right-click** to open the context menu:
  - **Click-Through** — toggles mouse transparency so clicks pass through the widget to whatever is behind it.
  - **Exit** — closes the application.
- The **X** button in the top-right corner also closes the widget.
- The widget does **not** appear in Alt+Tab.

## Important Notes

- The application **must run as Administrator**. The embedded app manifest requests elevation automatically.
- LibreHardwareMonitor requires admin privileges to access CPU/GPU temperature and load sensors.
- The widget updates every 1 second with minimal CPU overhead.

## Project Structure

```
ComputerLoadMonitoring/
├── Models/
│   └── HardwareData.cs           # Sensor data POCO
├── Services/
│   └── HardwareMonitorService.cs # LibreHardwareMonitor wrapper
├── ViewModels/
│   ├── ViewModelBase.cs          # INotifyPropertyChanged base
│   └── MainViewModel.cs         # Main VM with timer and properties
├── Views/
│   ├── MainWindow.xaml           # UI layout
│   └── MainWindow.xaml.cs        # Window behavior (drag, Alt+Tab hide)
├── Helpers/
│   ├── NativeMethods.cs          # Win32 P/Invoke declarations
│   └── StringToVisibilityConverter.cs
├── App.xaml / App.xaml.cs
├── app.manifest                  # Requests admin elevation
└── ComputerLoadMonitoring.csproj
```
