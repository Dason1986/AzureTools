using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpContainer
    {
        [JsonPropertyName("Id")] public string Id { get; set; }
        [JsonPropertyName("Names")] public List<string> Names { get; set; }
        [JsonPropertyName("Image")] public string Image { get; set; }
        [JsonPropertyName("State")] public string State { get; set; }
        [JsonPropertyName("Status")] public string Status { get; set; }
        [JsonPropertyName("Ports")] public JsonElement Ports { get; set; }
    }
}
