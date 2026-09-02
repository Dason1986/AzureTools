using System;
using System.Collections.Generic;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppDetail
    {
        public string Name { get; set; }
        public string EnvironmentName { get; set; }
        public string ProvisioningState { get; set; }
        public string RunningStatus { get; set; }
        public string Location { get; set; }
        public string LatestRevisionName { get; set; }
        public string LatestReadyRevisionName { get; set; }
        public string LatestRevisionFqdn { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? LastModifiedTime { get; set; }
        public List<ContainerAppRevisionInfo> Revisions { get; set; } = new List<ContainerAppRevisionInfo>();
        public List<ContainerAppReplicaInfo> Replicas { get; set; } = new List<ContainerAppReplicaInfo>();
        public ContainerAppScaleInfo Scale { get; set; }
        public ContainerAppIngress Ingress { get; set; }
        public List<ContainerAppVolumeInfo> Volumes { get; set; } = new List<ContainerAppVolumeInfo>();
        public List<ContainerAppEnvVarInfo> EnvironmentVariables { get; set; } = new List<ContainerAppEnvVarInfo>();
    }
}
