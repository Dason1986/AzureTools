using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class ContainerCreateConfig
    {
        [JsonPropertyName("Image")] public string Image { get; set; }
        [JsonPropertyName("Cmd")] public List<string> Cmd { get; set; }
        [JsonPropertyName("Env")] public List<string> Env { get; set; }
        [JsonPropertyName("Name")] public string Name { get; set; }
        [JsonPropertyName("Privileged")] public bool Privileged { get; set; }
        [JsonPropertyName("AutoRemove")] public bool AutoRemove { get; set; }
        [JsonPropertyName("RestartPolicy")] public string RestartPolicy { get; set; }
        [JsonPropertyName("Binds")] public List<string> Binds { get; set; }
        [JsonPropertyName("PortBindings")] public List<string> PortBindings { get; set; }
        [JsonPropertyName("Networks")] public List<string> Networks { get; set; }
    }
}
