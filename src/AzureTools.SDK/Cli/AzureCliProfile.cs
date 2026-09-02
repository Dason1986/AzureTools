using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureTools.Cli
{
    public class AzureCliProfile
    {
        [JsonPropertyName("azure-cli")]
        public string AzureCli { get; set; }
        [JsonPropertyName("azure-cli-core")]
        public string AzureCliCore { get; set; }
        [JsonPropertyName("azure-cli-telemetry")]
        public string AzureClitTlemetry { get; set; }
        [JsonPropertyName("extensions")]
        public Dictionary<string, string> Extensions { get; set; }
    }


}
