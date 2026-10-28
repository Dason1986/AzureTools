namespace Toolkit.Modules.SSH.Models.Features
{
    public class PodmanFeatureConfig
    {
        public int PodmanPort { get; set; } = 2375;
        public string PodmanApiMode { get; set; } = SshDockerApiMode.Cli;
    }
}
