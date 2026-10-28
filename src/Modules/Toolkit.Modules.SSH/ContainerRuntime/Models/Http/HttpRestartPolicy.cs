using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpRestartPolicy
    {
        [JsonPropertyName("Name")] public string Name { get; set; }
    }
}
