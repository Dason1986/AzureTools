using AzureTools.Cli;
using AzureTools.ViewModels;

namespace AzureTools.Module.Comm.ViewModels.CliStreamSession
{
    public class CliStreamSessionOption : IViewModelOption
    {
        public ICliStreamSession Session { get; set; }

        public string Title { get; set; }

        public bool IsInteractive { get; set; }
    }
}
