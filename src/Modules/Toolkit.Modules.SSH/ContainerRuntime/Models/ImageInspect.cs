using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class ImageInspect
    {
        [JsonPropertyName("RepoTags")] public List<string> RepoTags { get; set; }
        [JsonPropertyName("Config")] public ImageInspectConfig Config { get; set; }
    }
}
