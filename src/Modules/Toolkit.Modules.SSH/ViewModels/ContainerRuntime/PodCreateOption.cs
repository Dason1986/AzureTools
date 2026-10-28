using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class PodCreateOption : IViewModelOption
    {
        public string Engine { get; set; } = SshContainerEngine.Podman;
    }
}
