using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class Pod
    {
        [JsonPropertyName("Id")] public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        [JsonPropertyName("NumContainers")] public int NumContainers { get; set; }
        [JsonPropertyName("ContainersNames")] public List<string> ContainerNames { get; set; }
    }
}
