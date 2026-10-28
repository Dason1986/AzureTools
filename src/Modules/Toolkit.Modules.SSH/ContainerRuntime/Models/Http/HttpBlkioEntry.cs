using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpBlkioEntry
    {
        [JsonPropertyName("op")] public string Op { get; set; }
        [JsonPropertyName("value")] public long Value { get; set; }
    }
}
