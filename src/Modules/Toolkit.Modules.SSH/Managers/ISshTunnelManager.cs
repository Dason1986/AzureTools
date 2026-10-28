using System;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    public interface ISshTunnelManager : IDisposable
    {
        Task<int> OpenAsync(SshHostProfile host, string remoteHost, int remotePort, CancellationToken ct = default);

        Task CloseAsync(Guid hostId, string remoteHost, int remotePort);

        Task CloseAllAsync(Guid hostId);
    }
}
