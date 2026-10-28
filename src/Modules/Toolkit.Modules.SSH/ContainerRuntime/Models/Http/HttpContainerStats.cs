using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpContainerStats
    {
        [JsonPropertyName("cpu_stats")] public HttpCpuStats CpuStats { get; set; }
        [JsonPropertyName("precpu_stats")] public HttpCpuStats PreCpuStats { get; set; }
        [JsonPropertyName("memory_stats")] public HttpMemoryStats MemoryStats { get; set; }
        [JsonPropertyName("blkio_stats")] public HttpBlkioStats BlkioStats { get; set; }
        [JsonPropertyName("networks")] public Dictionary<string, HttpNetworkStats> Networks { get; set; }
    }

    public class HttpCpuStats
    {
        [JsonPropertyName("cpu_usage")] public HttpCpuUsage CpuUsage { get; set; }
        [JsonPropertyName("system_cpu_usage")] public long SystemCpuUsage { get; set; }
    }

    public class HttpCpuUsage
    {
        [JsonPropertyName("total_usage")] public long TotalUsage { get; set; }
    }

    public class HttpMemoryStats
    {
        [JsonPropertyName("usage")] public long Usage { get; set; }
        [JsonPropertyName("limit")] public long Limit { get; set; }
    }
}
