using Azure.ResourceManager.AppContainers.Models;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppIngress
    {
        public ContainerAppIngressTransportMethod? Transport { get; set; }
        public bool? External { get; set; }
        public int? ExposedPort { get; set; }
    }
}
