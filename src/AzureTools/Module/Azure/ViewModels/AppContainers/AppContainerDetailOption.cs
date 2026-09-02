using AzureTools.Models;
using AzureTools.SDK.AppContainers;
using AzureTools.ViewModels;

namespace AzureTools.Module.Azure.ViewModels.AppContainers
{
    public class AppContainerDetailOption : IViewModelOption
    {
        public ContainerAppInfo ContainerApp { get; set; }

        public IAppContainerManagement Management { get; set; }
        public AppContainerEnv EnvOption { get; set; }
    }
}
