using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;

namespace Toolkit.Modules.SSH.ContainerRuntime
{
    public sealed class DefaultContainerRuntimeManagerFactory : IContainerRuntimeManagerFactory
    {
        private readonly ISshConnectionManager _connectionManager;
        private readonly ISshTunnelManager _tunnel;

        public DefaultContainerRuntimeManagerFactory(ISshConnectionManager connectionManager, ISshTunnelManager tunnel)
        {
            _connectionManager = connectionManager;
            _tunnel = tunnel;
        }

        public IContainerRuntimeManager Create(SshHostProfile host, string engine = SshContainerEngine.Docker)
        {
            if (SshContainerEngine.IsPodman(engine))
            {
                PodmanFeatureConfig podman = host.GetPodmanConfig();
                return podman.PodmanApiMode == SshDockerApiMode.Http
                    ? new HttpContainerRuntimeManager(_tunnel, host, podman.PodmanPort)
                    : new PodmanCliContainerRuntimeManager(_connectionManager, host);
            }

            DockerFeatureConfig docker = host.GetDockerConfig();
            return docker.DockerApiMode == SshDockerApiMode.Http
                ? new HttpContainerRuntimeManager(_tunnel, host, docker.DockerPort)
                : new DockerCliContainerRuntimeManager(_connectionManager, host);
        }
    }
}
