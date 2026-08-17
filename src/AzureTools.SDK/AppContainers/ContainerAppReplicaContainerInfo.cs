namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppReplicaContainerInfo
    {
        public string Name { get; set; }
        public string ContainerId { get; set; }
        public bool? IsReady { get; set; }
        public bool? IsStarted { get; set; }
        public int? RestartCount { get; set; }
        public string RunningState { get; set; }
        public string RunningStateDetails { get; set; }
        public string LogStreamEndpoint { get; set; }
        public string ExecEndpoint { get; set; }
    }
}
