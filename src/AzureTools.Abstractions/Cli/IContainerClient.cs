using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface IContainerClient
    {
        /// <summary>docker exec -it 交互式，直接创建交互式Shell流</summary>
        ICliStreamSession Exec(string shell = "/bin/bash", CancellationToken ct = default);

        Task<CliResult> Stop(CancellationToken ct = default);
        Task<CliResult> Start(CancellationToken ct = default);
        Task<CliResult> Restart(CancellationToken ct = default);
        Task<CliResult> Logs(string extraArgs = null, CancellationToken ct = default);
        Task<CliResult> CopyToContainer(string hostSource, string containerDest, CancellationToken ct = default);

        Task<CliResult> CopyFromContainer(string containerSource, string hostDest, CancellationToken ct = default);
        Task<CliResult> Inspect(CancellationToken ct = default);
    }

}
