using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Managers
{
    public class DefaultSshConnectionTestService : ISshConnectionTestService
    {
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

        public async Task<IDictionary<string, string>> GetProfileAsync(SshHostProfile host)
        {
            ArgumentNullException.ThrowIfNull(host);

            return await Task.Run(() =>
            {
                using SshClient client = CreateClient(host);
                client.Connect();
                using SshCommand cmd = client.CreateCommand("cat /etc/os-release 2>/dev/null || uname -a");
                cmd.CommandTimeout = TimeSpan.FromSeconds(15);
                string output = cmd.Execute();
                client.Disconnect();
                return ExtractOsVersion(output);
            }).ConfigureAwait(false);
        }

        private SshClient CreateClient(SshHostProfile host)
        {
            ConnectionInfo connectionInfo = new ConnectionInfo(host.Host, host.Port, host.UserName, CreateAuthMethod(host))
            {
                Timeout = ConnectTimeout
            };
            return new SshClient(connectionInfo);
        }

        protected virtual AuthenticationMethod CreateAuthMethod(SshHostProfile host)
        {
            if (string.Equals(host.AuthMode, nameof(SshAuthMode.PrivateKey), StringComparison.OrdinalIgnoreCase))
            {
                MemoryStream keyStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(host.PrivateKeyRawData));
                return string.IsNullOrEmpty(host.Passphrase)
                    ? new PrivateKeyAuthenticationMethod(host.UserName, new PrivateKeyFile(keyStream))
                    : new PrivateKeyAuthenticationMethod(host.UserName, new PrivateKeyFile(keyStream, host.Passphrase));
            }
            return new PasswordAuthenticationMethod(host.UserName, host.Password);
        }

        protected virtual IDictionary<string, string> ExtractOsVersion(string raw)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw)) return dic;


            foreach (string line in raw.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] trimmed = line.Trim().Split('=');
                dic.TryAdd(trimmed[0], trimmed[1].Replace("\"", string.Empty));
            }


            return dic;
        }

        private static string TrimQuotes(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length >= 2 && value[0] == '"' && value[^1] == '"'
                ? value.Substring(1, value.Length - 2)
                : value;
        }
    }
}
