using AzureTools.SDK.AppContainers;
using AzureTools.ViewModels;
using Toolkit.Modules.Azure.Models;

namespace Toolkit.Modules.Azure.ViewModels.AppContainers
{
    public class ContainerAppReplicaDetailOption : IViewModelOption
    {
        public ContainerAppReplicaInfo Replica { get; set; }

        public string ContainerAppName { get; set; }

        public AppContainerEnv EnvOption { get; set; }
        public IAppContainerManagement Management { get; set; }
    }
}
