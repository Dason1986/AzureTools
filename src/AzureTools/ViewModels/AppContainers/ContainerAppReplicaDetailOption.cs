using AzureTools.SDK.AppContainers;

namespace AzureTools.ViewModels.AppContainers
{
    public class ContainerAppReplicaDetailOption : IViewModelOption
    {
        public ContainerAppReplicaInfo Replica { get; set; }

        public string ContainerAppName { get; set; }

        public string ResourceGroupName { get; set; }
    }
}
