using System.Management;
using ComputerLoadMonitoring.Models;
using LibreHardwareMonitor.Hardware;

namespace ComputerLoadMonitoring.Services;

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;
    private bool _disposed;
    private bool _useWmiForCpuTemp;

    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true
        };
        _computer.Open();
    }

    public HardwareData GetHardwareData()
    {
        float cpuLoad = 0, cpuTemp = 0, gpuLoad = 0, gpuTemp = 0, ramUsage = 0, ramUsed = 0, ramTotal = 0;

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            foreach (var sub in hardware.SubHardware)
                sub.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    cpuLoad = GetSensorValue(hardware, SensorType.Load, "CPU Total") ?? cpuLoad;
                    if (!_useWmiForCpuTemp)
                    {
                        cpuTemp = GetSensorValue(hardware, SensorType.Temperature, "CPU Package")
                                  ?? GetSensorValue(hardware, SensorType.Temperature, "Core Average")
                                  ?? GetSensorValue(hardware, SensorType.Temperature, "Tctl")
                                  ?? GetSensorValue(hardware, SensorType.Temperature, "Tdie")
                                  ?? GetFirstSensorValue(hardware, SensorType.Temperature)
                                  ?? cpuTemp;
                    }
                    break;

                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    gpuLoad = GetSensorValue(hardware, SensorType.Load, "GPU Core") ?? gpuLoad;
                    gpuTemp = GetSensorValue(hardware, SensorType.Temperature, "GPU Core")
                              ?? GetFirstSensorValue(hardware, SensorType.Temperature)
                              ?? gpuTemp;
                    break;

                case HardwareType.Memory:
                    ramUsage = GetSensorValue(hardware, SensorType.Load, "Memory") ?? ramUsage;
                    ramUsed = GetSensorValue(hardware, SensorType.Data, "Memory Used") ?? ramUsed;
                    var ramAvailable = GetSensorValue(hardware, SensorType.Data, "Memory Available") ?? 0;
                    if (ramUsed > 0 && ramAvailable > 0)
                        ramTotal = ramUsed + ramAvailable;
                    break;
            }
        }

        if (cpuTemp == 0)
            cpuTemp = GetCpuTempFromMotherboard();

        if (cpuTemp == 0)
        {
            cpuTemp = GetCpuTempFromWmi();
            if (cpuTemp > 0)
                _useWmiForCpuTemp = true;
        }

        return new HardwareData
        {
            CpuLoad = cpuLoad,
            CpuTemp = cpuTemp,
            GpuLoad = gpuLoad,
            GpuTemp = gpuTemp,
            RamUsage = ramUsage,
            RamUsed = ramUsed,
            RamTotal = ramTotal
        };
    }

    private float GetCpuTempFromMotherboard()
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType != HardwareType.Motherboard)
                continue;

            foreach (var sub in hardware.SubHardware)
            {
                var temp = GetSensorValue(sub, SensorType.Temperature, "CPU")
                           ?? GetSensorValue(sub, SensorType.Temperature, "Tctl")
                           ?? GetSensorValue(sub, SensorType.Temperature, "Tdie");
                if (temp is > 0)
                    return temp.Value;
            }

            var mbTemp = GetSensorValue(hardware, SensorType.Temperature, "CPU");
            if (mbTemp is > 0)
                return mbTemp.Value;
        }

        return 0;
    }

    private static float GetCpuTempFromWmi()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\WMI",
                "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (var obj in searcher.Get())
            {
                var kelvinTenths = Convert.ToDouble(obj["CurrentTemperature"]);
                var celsius = (float)((kelvinTenths / 10.0) - 273.15);
                if (celsius is > 0 and < 150)
                    return celsius;
            }
        }
        catch { }

        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\CIMV2",
                "SELECT * FROM Win32_PerfFormattedData_Counters_ThermalZoneInformation");

            foreach (var obj in searcher.Get())
            {
                var kelvin = Convert.ToDouble(obj["Temperature"]);
                var celsius = (float)(kelvin - 273.15);
                if (celsius is > 0 and < 150)
                    return celsius;
            }
        }
        catch { }

        return 0;
    }

    private static float? GetSensorValue(IHardware hardware, SensorType type, string nameContains)
    {
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == type && sensor.Name.Contains(nameContains, StringComparison.OrdinalIgnoreCase))
                return sensor.Value;
        }

        foreach (var sub in hardware.SubHardware)
        {
            foreach (var sensor in sub.Sensors)
            {
                if (sensor.SensorType == type && sensor.Name.Contains(nameContains, StringComparison.OrdinalIgnoreCase))
                    return sensor.Value;
            }
        }

        return null;
    }

    private static float? GetFirstSensorValue(IHardware hardware, SensorType type)
    {
        foreach (var sensor in hardware.Sensors)
        {
            if (sensor.SensorType == type && sensor.Value.HasValue)
                return sensor.Value;
        }

        foreach (var sub in hardware.SubHardware)
        {
            foreach (var sensor in sub.Sensors)
            {
                if (sensor.SensorType == type && sensor.Value.HasValue)
                    return sensor.Value;
            }
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _computer.Close();
    }
}
