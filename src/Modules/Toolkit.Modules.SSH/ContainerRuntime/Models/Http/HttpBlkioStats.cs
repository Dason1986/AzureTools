using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpBlkioStats
    {
        [JsonPropertyName("io_service_bytes_recursive")] public HttpBlkioEntry[] IoServiceBytesRecursive { get; set; }
    }
}
