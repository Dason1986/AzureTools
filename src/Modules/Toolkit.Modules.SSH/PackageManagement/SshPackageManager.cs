using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.PackageManagement
{
    public class SshPackageManager : IPackageManager
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan DetectTimeout = TimeSpan.FromSeconds(20);
        private static readonly int[] NoResultExitCodes = { 1, 104 };
        private readonly ISshConnectionManager _connectionManager;
        private readonly SshHostProfile _host;

        public PackageManagerCatalog Catalog { get; private set; }

        public SshPackageManager(ISshConnectionManager connectionManager, SshHostProfile host)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task<PackageManagerCatalog> DetectAsync(CancellationToken ct = default)
        {
            PackageManagerCatalog preferred = PackageManagerCatalog.Resolve(_host.OsId, _host.OsVersion);
            string probed = await RunAsync(BuildProbeCommand(), ct, DetectTimeout).ConfigureAwait(false);
            string[] binaries = probed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (preferred != null && binaries.Contains(preferred.Binary))
            {
                Catalog = preferred;
                return Catalog;
            }
            Catalog = binaries.Select(PackageManagerCatalog.Find).FirstOrDefault(c => c != null);
            return Catalog;
        }

        public async Task<IReadOnlyList<PackageInfo>> SearchAsync(string keyword, CancellationToken ct = default)
        {
            PackageManagerCatalog catalog = RequireCatalog();
            string output = await RunAsync(Format(catalog.SearchCommandFormat, keyword), ct, null, NoResultExitCodes).ConfigureAwait(false);
            return catalog.SearchParser(output);
        }

        public async Task<IReadOnlyList<PackageInfo>> ListInstalledAsync(CancellationToken ct = default)
        {
            PackageManagerCatalog catalog = RequireCatalog();
            string output = await RunAsync(catalog.InstalledCommand, ct).ConfigureAwait(false);
            return catalog.InstalledParser(output);
        }

        public Task<PackageOperationResult> InstallAsync(string name, CancellationToken ct = default)
            => RunPackageAsync(RequireCatalog().InstallCommandFormat, name, null, ct);

        public Task<PackageOperationResult> RemoveAsync(string name, CancellationToken ct = default)
            => RunPackageAsync(RequireCatalog().RemoveCommandFormat, name, null, ct);

        public Task<PackageOperationResult> UpgradeAsync(string name, CancellationToken ct = default)
            => RunPackageAsync(RequireCatalog().UpgradeCommandFormat, name, null, ct);

        public Task<PackageOperationResult> DowngradeAsync(string name, string version, CancellationToken ct = default)
            => RunPackageAsync(RequireCatalog().DowngradeCommandFormat, name, version, ct);

        private static string BuildProbeCommand()
            => "for b in apt-get dnf yum apk zypper pacman; do command -v $b >/dev/null 2>&1 && echo $b; done; exit 0";

        private async Task<PackageOperationResult> RunPackageAsync(string format, string name, string version, CancellationToken ct)
        {
            if (format == null)
                throw new InvalidOperationException("当前包管理器不支持该操作");
            if (format.Contains("{1}") && string.IsNullOrEmpty(version))
                throw new InvalidOperationException("该操作需要指定目标版本");
            string command = string.Format(format, ShellQuote(name), string.IsNullOrEmpty(version) ? null : ShellQuote(version));
            return await ExecuteAsync(command, ct).ConfigureAwait(false);
        }

        private PackageManagerCatalog RequireCatalog()
            => Catalog ?? throw new InvalidOperationException("未识别远端主机的包管理器");

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

        private static string Format(string format, string argument)
            => string.Format(format, ShellQuote(argument));

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
