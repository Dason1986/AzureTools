using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class Network
    {
        [JsonPropertyName("ID")] public string Id { get; set; }
        public string Name { get; set; }
        public string Driver { get; set; }
        public string Scope { get; set; }
    }
}
