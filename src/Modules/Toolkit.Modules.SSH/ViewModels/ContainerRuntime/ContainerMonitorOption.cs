using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class ContainerMonitorOption : IViewModelOption
    {
        public string ContainerId { get; set; }
        public string ContainerName { get; set; }
        public string Engine { get; set; } = SshContainerEngine.Docker;
    }
}
