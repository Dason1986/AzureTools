using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpContainerCreateRequest
    {
        [JsonPropertyName("Image")] public string Image { get; set; }
        [JsonPropertyName("Cmd")] public List<string> Cmd { get; set; }
        [JsonPropertyName("Env")] public List<string> Env { get; set; }
        [JsonPropertyName("HostConfig")] public HttpHostConfig HostConfig { get; set; }
    }
}
