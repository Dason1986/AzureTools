using AzureTools.SDK.AppContainers;
using AzureTools.ViewModels;
using Toolkit.Modules.Azure.Models;

namespace Toolkit.Modules.Azure.ViewModels.AppContainers
{
    public class AppContainerDetailOption : IViewModelOption
    {
        public ContainerAppInfo ContainerApp { get; set; }

        public IAppContainerManagement Management { get; set; }
        public AppContainerEnv EnvOption { get; set; }
    }
}
