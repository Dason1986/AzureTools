using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class ImageInspectConfig
    {
        [JsonPropertyName("Env")] public List<string> Env { get; set; }
        [JsonPropertyName("ExposedPorts")] public Dictionary<string, JsonElement> ExposedPorts { get; set; }
        [JsonPropertyName("Labels")] public Dictionary<string, string> Labels { get; set; }
    }
}
