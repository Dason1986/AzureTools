using System;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppRevisionInfo
    {
        public string Name { get; set; }
        public string ProvisioningState { get; set; }
        public string RunningState { get; set; }
        public string HealthState { get; set; }
        public bool IsActive { get; set; }
        public int? Replicas { get; set; }
        public int? TrafficWeight { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastActiveOn { get; set; }
        public string Fqdn { get; set; }
    }
}
