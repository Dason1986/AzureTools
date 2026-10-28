using CommunityToolkit.Mvvm.ComponentModel;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class MountEntry : ObservableObject
    {
        [ObservableProperty]
        private string _hostPath;

        [ObservableProperty]
        private string _containerPath;

        [ObservableProperty]
        private bool _isReadOnly;
    }
}
