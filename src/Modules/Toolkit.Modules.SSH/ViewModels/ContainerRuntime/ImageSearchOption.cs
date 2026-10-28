using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class ImageSearchOption : IViewModelOption
    {
        public string Engine { get; set; } = SshContainerEngine.Docker;
    }
}
