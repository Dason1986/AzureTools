using System.Collections.Generic;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public sealed class DockerFeatureTabViewModel : ContainerEngineFeatureTabViewModel
    {
        private DockerFeatureConfig _config = new();

        public override string Code => SshHostFeature.Docker;

        public override string DisplayName => SSHResource.FeatureDocker;

        protected override void LoadConfig(Dictionary<string, string> features)
        {
            _config = DeserializeConfig(features, Code, SshJsonSerializerContext.Default.DockerFeatureConfig);
            Port = _config.DockerPort;
            ApiMode = string.IsNullOrEmpty(_config.DockerApiMode) ? SshDockerApiMode.Cli : _config.DockerApiMode;
        }

        protected override void SaveConfig(Dictionary<string, string> features)
        {
            _config.DockerPort = Port;
            _config.DockerApiMode = ApiMode;
            SerializeConfig(features, Code, _config, SshJsonSerializerContext.Default.DockerFeatureConfig);
        }
    }
}
