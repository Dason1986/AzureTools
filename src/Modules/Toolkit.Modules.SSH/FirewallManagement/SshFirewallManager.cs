using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.FirewallManagement
{
    public class SshFirewallManager : IFirewallManager
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan DetectTimeout = TimeSpan.FromSeconds(20);
        private readonly ISshConnectionManager _connectionManager;
        private readonly SshHostProfile _host;

        public FirewallBackendCatalog Catalog { get; private set; }

        public SshFirewallManager(ISshConnectionManager connectionManager, SshHostProfile host)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task<FirewallBackendCatalog> DetectAsync(CancellationToken ct = default)
        {
            string probed = await RunAsync(BuildProbeCommand(), ct, DetectTimeout).ConfigureAwait(false);
            string[] binaries = probed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Catalog = binaries.Select(FindByBinary).FirstOrDefault(c => c != null);
            return Catalog;
        }

        public async Task<bool> IsEnabledAsync(CancellationToken ct = default)
        {
            FirewallBackendCatalog catalog = RequireCatalog();
            string output = await RunAsync(catalog.StatusCommand, ct, CommandTimeout, new[] { 1, 101, 252 }).ConfigureAwait(false);
            return catalog.StatusParser(output);
        }

        public async Task<IReadOnlyList<FirewallRule>> ListRulesAsync(CancellationToken ct = default)
        {
            FirewallBackendCatalog catalog = RequireCatalog();
            string output = await RunAsync(catalog.ListCommand, ct).ConfigureAwait(false);
            return catalog.ListParser(output);
        }

        public Task<PackageOperationResult> AddRuleAsync(string port, string protocol, bool allow, CancellationToken ct = default)
        {
            FirewallBackendCatalog catalog = RequireCatalog();
            if (string.IsNullOrWhiteSpace(port)) throw new InvalidOperationException("请输入端口号");
            string command = string.Format(catalog.AddCommandFormat, ShellQuote(port), ShellQuote(protocol), allow ? "allow" : "deny", allow ? "ACCEPT" : "DROP");
            return ExecuteAsync(command, ct);
        }

        public Task<PackageOperationResult> RemoveRuleAsync(FirewallRule rule, CancellationToken ct = default)
        {
            FirewallBackendCatalog catalog = RequireCatalog();
            string command = BuildRemoveCommand(catalog, rule);
            return ExecuteAsync(command, ct);
        }

        public Task<PackageOperationResult> SetEnabledAsync(bool enabled, CancellationToken ct = default)
        {
            FirewallBackendCatalog catalog = RequireCatalog();
            string command = enabled ? catalog.EnableCommand : catalog.DisableCommand;
            if (command == null) throw new InvalidOperationException("当前防火墙不支持启用/停用操作");
            return ExecuteAsync(command, ct);
        }

        private static string BuildRemoveCommand(FirewallBackendCatalog catalog, FirewallRule rule)
        {
            if (catalog.RemoveByNumberCommandFormat != null && !string.IsNullOrEmpty(rule.Number))
                return string.Format(catalog.RemoveByNumberCommandFormat, rule.Number);
            if (catalog.RemoveByPortCommandFormat != null && !string.IsNullOrEmpty(rule.Port) && rule.Protocol != "service")
                return string.Format(catalog.RemoveByPortCommandFormat, ShellQuote(rule.Port), ShellQuote(rule.Protocol ?? "tcp"));
            throw new InvalidOperationException("当前规则不支持删除");
        }

        private static string BuildProbeCommand()
            => "for b in ufw firewall-cmd iptables; do command -v $b >/dev/null 2>&1 && echo $b; done; exit 0";

        private static FirewallBackendCatalog FindByBinary(string binary)
            => FirewallBackendCatalog.All.FirstOrDefault(c => c.Binary == binary);

        private FirewallBackendCatalog RequireCatalog()
            => Catalog ?? throw new InvalidOperationException("未识别远端主机的防火墙工具");

        private async Task<PackageOperationResult> ExecuteAsync(string command, CancellationToken ct)
        {
            SshClient client = _connectionManager.GetClient(_host);
            return await Task.Run(() =>
            {
                PackageOperationResult result = new PackageOperationResult { Command = command };
                try
                {
                    using SshCommand cmd = client.CreateCommand(command);
                    cmd.CommandTimeout = CommandTimeout;
                    cmd.Execute();
                    result.ExitStatus = cmd.ExitStatus ?? -1;
                    result.Output = cmd.Result;
                    result.Error = cmd.Error;
                    result.Success = cmd.ExitStatus == 0;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                }
                return result;
            }, ct).ConfigureAwait(false);
        }

        private static string ShellQuote(string value)
            => "'" + value.Replace("'", "'\\''") + "'";

        private async Task<string> RunAsync(string command, CancellationToken ct, TimeSpan? timeout = null, int[] toleratedExitCodes = null)
        {
            SshClient client = _connectionManager.GetClient(_host);
            return await Task.Run(() =>
            {
                using SshCommand cmd = client.CreateCommand(command);
                cmd.CommandTimeout = timeout ?? CommandTimeout;
                cmd.Execute();
                if (cmd.ExitStatus != 0 && Array.IndexOf(toleratedExitCodes ?? Array.Empty<int>(), cmd.ExitStatus) < 0)
                {
                    throw new InvalidOperationException(string.IsNullOrEmpty(cmd.Error) ? $"命令执行失败（exit {cmd.ExitStatus}）：{command}" : cmd.Error);
                }
                return cmd.Result;
            }, ct).ConfigureAwait(false);
        }
    }
}
