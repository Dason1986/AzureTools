using System.Collections.Generic;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public sealed class PodmanFeatureTabViewModel : ContainerEngineFeatureTabViewModel
    {
        private PodmanFeatureConfig _config = new();

        public override string Code => SshHostFeature.Podman;

        public override string DisplayName => SSHResource.FeaturePodman;

        protected override void LoadConfig(Dictionary<string, string> features)
        {
            _config = DeserializeConfig(features, Code, SshJsonSerializerContext.Default.PodmanFeatureConfig);
            Port = _config.PodmanPort;
            ApiMode = string.IsNullOrEmpty(_config.PodmanApiMode) ? SshDockerApiMode.Cli : _config.PodmanApiMode;
        }

        protected override void SaveConfig(Dictionary<string, string> features)
        {
            _config.PodmanPort = Port;
            _config.PodmanApiMode = ApiMode;
            SerializeConfig(features, Code, _config, SshJsonSerializerContext.Default.PodmanFeatureConfig);
        }
    }
}
