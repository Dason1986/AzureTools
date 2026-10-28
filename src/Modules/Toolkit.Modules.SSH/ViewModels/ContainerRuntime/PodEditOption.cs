using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class PodEditOption : IViewModelOption
    {
        public string Engine { get; set; } = SshContainerEngine.Podman;

        public string PodName { get; set; }
    }
}
