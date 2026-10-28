using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    public sealed class DefaultSshConnectionManager : ISshConnectionManager
    {
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(15);
        private readonly ConcurrentDictionary<Guid, SshClient> clients = new();
        private readonly ConcurrentDictionary<Guid, SftpClient> sftpClients = new();
        private readonly Lazy<ISshTunnelManager> _tunnel;

        public DefaultSshConnectionManager(Lazy<ISshTunnelManager> tunnel)
        {
            _tunnel = tunnel;
        }

        public SshClient GetClient(SshHostProfile host)
        {
            ArgumentNullException.ThrowIfNull(host);
            SshClient client = clients.GetOrAdd(host.Id, _ => CreateClient(host));
            lock (client) EnsureConnected(client);
            return client;
        }

        public SftpClient GetSftpClient(SshHostProfile host)
        {
            ArgumentNullException.ThrowIfNull(host);
            SftpClient client = sftpClients.GetOrAdd(host.Id, _ => CreateSftpClient(host));
            lock (client) EnsureConnected(client);
            return client;
        }

        public void Disconnect(Guid hostId)
        {
            if (clients.TryRemove(hostId, out SshClient ssh)) DisposeClient(ssh);
            if (sftpClients.TryRemove(hostId, out SftpClient sftp)) DisposeClient(sftp);
            try { _tunnel.Value.CloseAllAsync(hostId).GetAwaiter().GetResult(); } catch { }
        }

        public void DisconnectAll()
        {
            foreach (Guid id in clients.Keys)
            {
                if (clients.TryRemove(id, out SshClient ssh)) DisposeClient(ssh);
            }
            foreach (Guid id in sftpClients.Keys)
            {
                if (sftpClients.TryRemove(id, out SftpClient sftp)) DisposeClient(sftp);
            }
        }

        public void Dispose() => DisconnectAll();

        private static SshClient CreateClient(SshHostProfile host)
        {
            return new SshClient(CreateConnectionInfo(host));
        }

        private static SftpClient CreateSftpClient(SshHostProfile host)
        {
            return new SftpClient(CreateConnectionInfo(host));
        }

        private static ConnectionInfo CreateConnectionInfo(SshHostProfile host)
        {
            return new ConnectionInfo(host.Host, host.Port, host.UserName, CreateAuthMethod(host))
            {
                Timeout = ConnectTimeout
            };
        }

        private static AuthenticationMethod CreateAuthMethod(SshHostProfile host)
        {
            if (string.Equals(host.AuthMode, nameof(SshAuthMode.PrivateKey), StringComparison.OrdinalIgnoreCase))
            {
                return new PrivateKeyAuthenticationMethod(host.UserName, CreateKeyFile(host));
            }
            return new PasswordAuthenticationMethod(host.UserName, host.Password);
        }

        private static PrivateKeyFile CreateKeyFile(SshHostProfile host)
        {
            MemoryStream keyStream = new MemoryStream(Encoding.UTF8.GetBytes(host.PrivateKeyRawData));
            return string.IsNullOrEmpty(host.Passphrase)
                ? new PrivateKeyFile(keyStream)
                : new PrivateKeyFile(keyStream, host.Passphrase);
        }

        private static void EnsureConnected(BaseClient client)
        {
            if (client.IsConnected) return;
            client.Connect();
        }

        private static void DisposeClient(BaseClient client)
        {
            if (client == null) return;
            try
            {
                if (client.IsConnected) client.Disconnect();
            }
            catch
            {
            }
            client.Dispose();
        }
    }
}
