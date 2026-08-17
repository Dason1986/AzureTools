using AzureTools.SDK.AppContainers;

namespace AzureTools.ViewModels.AppContainers
{
    public class AppContainerDetailOption : IViewModelOption
    {
        public ContainerAppInfo ContainerApp { get; set; }

        public IAppContainerManagement Management { get; set; }

        public string ResourceGroupName { get; set; }
    }
}
