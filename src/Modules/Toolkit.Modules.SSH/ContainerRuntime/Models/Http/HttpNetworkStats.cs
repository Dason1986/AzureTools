using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.ContainerRuntime.Models.Http
{
    public class HttpNetworkStats
    {
        [JsonPropertyName("rx_bytes")] public long RxBytes { get; set; }
        [JsonPropertyName("tx_bytes")] public long TxBytes { get; set; }
    }
}
