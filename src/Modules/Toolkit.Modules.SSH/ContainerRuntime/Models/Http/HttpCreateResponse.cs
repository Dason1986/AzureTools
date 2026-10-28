using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpCreateResponse
    {
        [JsonPropertyName("Id")] public string Id { get; set; }
    }
}
