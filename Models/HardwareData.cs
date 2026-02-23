namespace ComputerLoadMonitoring.Models;

public sealed class HardwareData
{
    public float CpuLoad { get; init; }
    public float CpuTemp { get; init; }
    public float GpuLoad { get; init; }
    public float GpuTemp { get; init; }
    public float RamUsage { get; init; }
    public float RamUsed { get; init; }
    public float RamTotal { get; init; }
}
