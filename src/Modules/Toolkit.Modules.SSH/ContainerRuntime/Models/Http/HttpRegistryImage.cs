using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpRegistryImage
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("star_count")] public int StarCount { get; set; }
        [JsonPropertyName("is_official")] public bool IsOfficial { get; set; }
    }
}
