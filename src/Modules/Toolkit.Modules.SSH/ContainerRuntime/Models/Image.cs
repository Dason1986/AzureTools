using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models
{
    public class Image
    {
        [JsonPropertyName("ID")] public string Id { get; set; }
        [JsonPropertyName("Repository")] public string Repository { get; set; }
        [JsonPropertyName("Tag")] public string Tag { get; set; }
        [JsonPropertyName("Size")][JsonConverter(typeof(FlexibleStringJsonConverter))] public string Size { get; set; }
        public string RepoTag => string.IsNullOrEmpty(Tag) || Tag == "<none>" ? Repository : $"{Repository}:{Tag}";
    }
}
