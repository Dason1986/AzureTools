using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpImage
    {
        [JsonPropertyName("Id")] public string Id { get; set; }
        [JsonPropertyName("RepoTags")] public List<string> RepoTags { get; set; }
        [JsonPropertyName("Size")] public long Size { get; set; }
    }
}
