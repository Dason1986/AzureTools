using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ContainerRuntime
{
    public interface IContainerRuntimeManagerFactory
    {
        IContainerRuntimeManager Create(SshHostProfile host, string engine = SshContainerEngine.Docker);
    }
}
