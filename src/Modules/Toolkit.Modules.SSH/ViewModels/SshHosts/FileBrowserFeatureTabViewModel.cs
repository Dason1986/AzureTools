using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public partial class FileBrowserFeatureTabViewModel : SshFeatureTabViewModel
    {
        private FileBrowserFeatureConfig _config = new();

        public override string Code => SshHostFeature.FileBrowser;

        public override string DisplayName => SSHResource.FeatureFileBrowser;

        public FileBrowserFeatureTabViewModel()
        {
        }

        public override Task LoadAsync(SshHostProfile host)
        {
            if (host == null) return Task.CompletedTask;

            _config = DeserializeConfig(host.Features, Code, SshJsonSerializerContext.Default.FileBrowserFeatureConfig);
            IsEnabled = IsFeatureEnabled(host.Features);
            return Task.CompletedTask;
        }

        public override void Apply(SshHostProfile host)
        {
            if (host.Features == null) host.Features = new Dictionary<string, string>();

            if (IsEnabled)
            {
                SerializeConfig(host.Features, Code, _config, SshJsonSerializerContext.Default.FileBrowserFeatureConfig);
            }
            else
            {
                RemoveConfig(host.Features, Code);
            }
        }
    }
}
