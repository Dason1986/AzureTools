using AzureTools.Cli;
using AzureTools.ViewModels;

namespace AzureTools.Module.Comm.ViewModels.FileBrowser
{
    public class FileBrowserOption : IViewModelOption
    {
        public IContainerFileProvider FileProvider { get; set; }

        public string Title { get; set; }
    }
}
