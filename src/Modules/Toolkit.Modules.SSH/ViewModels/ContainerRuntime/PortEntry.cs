using CommunityToolkit.Mvvm.ComponentModel;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class PortEntry : ObservableObject
    {
        [ObservableProperty]
        private string _hostPort;

        [ObservableProperty]
        private string _containerPort;
    }
}
