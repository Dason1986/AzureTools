using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface IAppContainerClient
    {
        Task<CliResult> GetLogsAsync(string revision, string replica, CancellationToken ct = default);
        Task<ICliStreamSession> GetLogsFollowAsync(string revision, string replica, CancellationToken ct = default);
        Task<ICliStreamSession> ExecAsync(string revision, string replica, string container, CancellationToken ct = default);
        Task<IContainerFileProvider> GetFileBrowserAsync(string revision, string replica, string container, CancellationToken ct = default);
        Task SecretAddOrUpdateAsync(IDictionary<string, string> secrets, CancellationToken ct = default);
        Task SecretRemoveAsync(string[] secrets, CancellationToken ct = default);
        Task RevisionRestartAsync(string revision, CancellationToken ct = default);
    }
}
