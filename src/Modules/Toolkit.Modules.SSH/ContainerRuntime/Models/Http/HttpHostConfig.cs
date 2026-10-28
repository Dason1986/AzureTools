using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpHostConfig
    {
        [JsonPropertyName("Privileged")] public bool Privileged { get; set; }
        [JsonPropertyName("AutoRemove")] public bool AutoRemove { get; set; }
        [JsonPropertyName("RestartPolicy")] public HttpRestartPolicy RestartPolicy { get; set; }
        [JsonPropertyName("Binds")] public List<string> Binds { get; set; }
        [JsonPropertyName("PortBindings")] public Dictionary<string, List<HttpPortBinding>> PortBindings { get; set; }
    }
}
