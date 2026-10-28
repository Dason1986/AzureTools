using CommunityToolkit.Mvvm.ComponentModel;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public partial class SshFeatureOption : ObservableObject
    {
        public string Code { get; }

        public string Name { get; }

        [ObservableProperty]
        private bool _isEnabled;

        public SshFeatureOption(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
