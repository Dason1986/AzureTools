using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class NetworkDetailOption : IViewModelOption
    {
        public string NetworkId { get; set; }
        public string NetworkName { get; set; }
        public string Engine { get; set; } = SshContainerEngine.Docker;
    }
}
