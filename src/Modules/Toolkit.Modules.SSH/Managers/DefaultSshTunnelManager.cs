using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    public sealed class DefaultSshTunnelManager : ISshTunnelManager
    {
        private readonly ISshConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<TunnelKey, ForwardedPortLocal> _ports = new();

        public DefaultSshTunnelManager(ISshConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<int> OpenAsync(SshHostProfile host, string remoteHost, int remotePort, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(host);
            TunnelKey key = new TunnelKey(host.Id, remoteHost, remotePort);
            return await Task.Run(() =>
            {
                ForwardedPortLocal port = _ports.GetOrAdd(key, _ => CreatePort(host, remoteHost, remotePort));
                lock (port)
                {
                    if (!port.IsStarted) port.Start();
                    return (int)port.BoundPort;
                }
            }, ct).ConfigureAwait(false);
        }

        public Task CloseAsync(Guid hostId, string remoteHost, int remotePort)
        {
            return Task.Run(() =>
            {
                TunnelKey key = new TunnelKey(hostId, remoteHost, remotePort);
                if (_ports.TryRemove(key, out ForwardedPortLocal port)) DisposePort(port);
            });
        }

        public Task CloseAllAsync(Guid hostId)
        {
            return Task.Run(() =>
            {
                foreach (KeyValuePair<TunnelKey, ForwardedPortLocal> kv in _ports)
                {
                    if (kv.Key.HostId != hostId) continue;
                    if (_ports.TryRemove(kv.Key, out ForwardedPortLocal port)) DisposePort(port);
                }
            });
        }

        public void Dispose()
        {
            foreach (TunnelKey key in _ports.Keys)
            {
                if (_ports.TryRemove(key, out ForwardedPortLocal port)) DisposePort(port);
            }
        }

        private ForwardedPortLocal CreatePort(SshHostProfile host, string remoteHost, int remotePort)
        {
            SshClient client = _connectionManager.GetClient(host);
            ForwardedPortLocal port = new ForwardedPortLocal("127.0.0.1", 0, remoteHost, (uint)remotePort);
            lock (client)
            {
                client.AddForwardedPort(port);
            }
            return port;
        }

        private static void DisposePort(ForwardedPortLocal port)
        {
            if (port == null) return;
            try { if (port.IsStarted) port.Stop(); } catch { }
            try { port.Dispose(); } catch { }
        }

        private sealed record TunnelKey(Guid HostId, string RemoteHost, int RemotePort);
    }
}
