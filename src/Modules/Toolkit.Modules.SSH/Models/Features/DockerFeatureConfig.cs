namespace Toolkit.Modules.SSH.Models.Features
{
    public class DockerFeatureConfig
    {
        public int DockerPort { get; set; } = 2375;
        public string DockerApiMode { get; set; } = SshDockerApiMode.Cli;
    }
}
