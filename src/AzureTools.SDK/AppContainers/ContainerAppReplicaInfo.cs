using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppReplicaInfo
    {
        public string Name { get; set; }
        public string RevisionName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string RunningState { get; set; }
        public string RunningStateDetails { get; set; }
        public List<ContainerAppReplicaContainerInfo> Containers { get; set; } = new List<ContainerAppReplicaContainerInfo>();

        public string ContainerName => Containers.FirstOrDefault()?.Name;
        public bool? ContainerReady => Containers.FirstOrDefault()?.IsReady;
        public int? ContainerRestartCount => Containers.FirstOrDefault()?.RestartCount;
        public string ContainerRunningState => Containers.FirstOrDefault()?.RunningState;
    }
}
