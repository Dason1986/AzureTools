using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpPortBinding
    {
        [JsonPropertyName("HostPort")] public string HostPort { get; set; }
    }
}
