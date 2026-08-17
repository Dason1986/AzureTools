using System;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerConsoleInfo
    {
        public string ExecEndpoint { get; set; }
        public string LogStreamEndpoint { get; set; }
        public string AccessToken { get; set; }
        public string AzureAdToken { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string ContainerName { get; set; }
        public string ReplicaName { get; set; }
        public string RevisionName { get; set; }
        public string ContainerAppName { get; set; }
    }
}
