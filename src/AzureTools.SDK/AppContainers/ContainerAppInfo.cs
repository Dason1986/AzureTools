using AzureTools.SDK.Models;
using System;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppInfo : IStatus
    {
        public string Name { get; set; }
        public string EnvironmentName { get; set; }
        public string ProvisioningState { get; set; }
        public string RunningStatus { get; set; }
        public string LatestRevisionName { get; set; }
        public int? Replicas { get; set; }
        public string Image { get; set; }
        public string Location { get; set; }
        public string ResourceType { get; set; }
        public DateTime? RevisionCreatedTime { get; set; }

        string IStatus.Status => RunningStatus;
    }
}
