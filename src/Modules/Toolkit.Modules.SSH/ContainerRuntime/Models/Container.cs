using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class Container
    {
        [JsonPropertyName("ID")] public string Id { get; set; }
        [JsonPropertyName("Names")][JsonConverter(typeof(FlexibleStringJsonConverter))] public string Name { get; set; }
        [JsonPropertyName("Image")] public string Image { get; set; }
        [JsonPropertyName("State")] public string State { get; set; }
        [JsonPropertyName("Status")] public string Status { get; set; }
        [JsonPropertyName("IsInfra")] public bool IsInfra { get; set; }

        /// <summary>所属 Pod 的 Id（Podman 输出，Docker 为空）。</summary>
        [JsonPropertyName("Pod")] public string Pod { get; set; }
    }
}
