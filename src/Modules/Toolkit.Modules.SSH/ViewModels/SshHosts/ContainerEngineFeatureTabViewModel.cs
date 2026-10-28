using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Models;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public abstract partial class ContainerEngineFeatureTabViewModel : SshFeatureTabViewModel
    {
        [ObservableProperty]
        private int _port = 2375;

        [ObservableProperty]
        private string _apiMode = SshDockerApiMode.Cli;

        [ObservableProperty]
        private Option<string> _selectedApiMode;

        public Option<string>[] ApiModeOptions { get; }

        protected ContainerEngineFeatureTabViewModel()
        {
            ApiModeOptions = new[]
            {
                new Option<string>(SshDockerApiMode.Cli, SSHResource.DockerApiModeCli),
                new Option<string>(SshDockerApiMode.Http, SSHResource.DockerApiModeHttp)
            };
            _selectedApiMode = ApiModeOptions[0];
        }

        public override Task LoadAsync(SshHostProfile host)
        {
            if (host == null) return Task.CompletedTask;
            IsEnabled = IsFeatureEnabled(host.Features);
            LoadConfig(host.Features);
            SelectedApiMode = ApiModeOptions.FirstOrDefault(o => o.Value == ApiMode) ?? ApiModeOptions[0];
            return Task.CompletedTask;
        }

        public override void Apply(SshHostProfile host)
        {
            if (host.Features == null) host.Features = new Dictionary<string, string>();
            if (IsEnabled) SaveConfig(host.Features);
            else RemoveConfig(host.Features, Code);
        }

        partial void OnSelectedApiModeChanged(Option<string> value)
        {
            if (value != null) ApiMode = value.Value;
        }

        protected abstract void LoadConfig(Dictionary<string, string> features);

        protected abstract void SaveConfig(Dictionary<string, string> features);
    }
}
