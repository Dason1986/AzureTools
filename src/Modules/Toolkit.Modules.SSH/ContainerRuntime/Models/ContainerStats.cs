namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class ContainerStats
    {
        public double CpuPercent { get; set; }
        public long MemoryUsage { get; set; }
        public long MemoryLimit { get; set; }
        public double MemoryPercent => MemoryLimit > 0 ? (double)MemoryUsage / MemoryLimit * 100.0 : 0;
        public long BlockReadBytes { get; set; }
        public long BlockWriteBytes { get; set; }
        public long NetworkReceivedBytes { get; set; }
        public long NetworkTransmittedBytes { get; set; }
    }
}
