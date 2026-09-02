using AzureTools.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    class AppContainerClient : IAppContainerClient
    {
        public AppContainerClient(string azPath, string name, string resourceGroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup);
            this.azPath = azPath;
            Name = name;
            ResourceGroup = resourceGroup;
        }

        private readonly string azPath;

        public string Name { get; }
        public string ResourceGroup { get; }

        protected virtual string BuildMoudleArgs(string moudle, IDictionary<string, string> pairs)
        {
            string args = string.Join(" ", pairs.Select(x => $"{x.Key} {x.Value}".Trim()));
            return $"containerapp {moudle} {args}";

        }

        public async Task RevisionRestartAsync(string revision, CancellationToken ct = default)
        {
            IDictionary<string, string> dic = Comm(revision: revision, replica: null);
            string parms = BuildMoudleArgs("revision restart", dic);
            CliResult result = await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
            if (!result.Success && !string.IsNullOrEmpty(result.Error)) throw new AzureToolsException($"RevisionRestart failed {result.Error}");
        }

        public async Task SecretAddOrUpdateAsync(IDictionary<string, string> secrets, CancellationToken ct = default)
        {
            IDictionary<string, string> dic = Comm(null, null);
            dic.Add("--secrets", string.Join(" ", secrets.Select(x => $"{x.Key}={x.Value}")));
            string parms = BuildMoudleArgs("secret set", dic);
            CliResult result = await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
            if (!result.Success && !string.IsNullOrEmpty(result.Error)) throw new AzureToolsException($"SecretAddOrUpdate failed {result.Error}");
        }
        public async Task SecretRemoveAsync(string[] secrets, CancellationToken ct = default)
        {
            IDictionary<string, string> dic = Comm(null, null);
            dic.Add("--secret-names", string.Join(" ", secrets));
            string parms = BuildMoudleArgs("secret remove", dic);
            CliResult result = await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
            if (!result.Success && !string.IsNullOrEmpty(result.Error)) throw new AzureToolsException($"SecretRemove failed {result.Error}");
        }
        public async Task<CliResult> GetLogsAsync(string revision, string replica, CancellationToken ct = default)
        {
            string parms = BuildMoudleArgs("logs show", Comm(revision, replica));
            CliResult result = await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
            return result;
        }
        public async Task<ICliStreamSession> GetLogsFollowAsync(string revision, string replica, CancellationToken ct = default)
        {
            IDictionary<string, string> dic = Comm(revision, replica);
            dic.Add("--follow", "");
            string parms = BuildMoudleArgs("logs show", dic);
            return CliClientBuilder.Create(azPath).Args(parms).StartStream(ct);
        }

        public async Task<ICliStreamSession> ExecAsync(string revision, string replica, string container, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(container);
            IDictionary<string, string> dic = Comm(revision, replica);
            dic.Add("--container", container);
            string parms = BuildMoudleArgs("exec", dic);
            return CliClientBuilder.Create(azPath).Args(parms).EnableStandardInput().StartStream(ct);
        }

        public async Task<IContainerFileProvider> GetFileBrowserAsync(string revision, string replica, string container, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(container);
            ICliStreamSession session = await ExecAsync(revision, replica, container, ct);
            return new DefaultContainerFileProvider(session);
        }
        protected virtual IDictionary<string, string> Comm(string revision, string replica)
        {

            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "--name",Name},
                { "--resource-group",ResourceGroup}
            };
            if (!string.IsNullOrEmpty(revision)) dic.Add("--revision", revision);
            if (!string.IsNullOrEmpty(replica)) dic.Add("--replica", replica);
            return dic;
        }
    }
}
