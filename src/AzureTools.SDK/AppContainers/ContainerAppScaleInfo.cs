using System.Collections.Generic;

namespace AzureTools.SDK.AppContainers
{
    public class ContainerAppScaleInfo
    {
        public int? MinReplicas { get; set; }
        public int? MaxReplicas { get; set; }
        public string CooldownPeriod { get; set; }
        public string PollingInterval { get; set; }
        public List<string> ScaleRules { get; set; } = new List<string>();
    }
}
