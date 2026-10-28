using AzureTools.ViewModels;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class ImageTagsOption : IViewModelOption
    {
        public string ImageId { get; set; }
        public string RepoTag { get; set; }
        public string Engine { get; set; } = SshContainerEngine.Docker;
    }
}
