using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface IContainerShell
    {
        Task<string> CatAsync(string filePath, CancellationToken ct = default);
        void Disconnect();
        Task<List<ContainerFileEntry>> LsAsync(string path = ".", CancellationToken ct = default);
        Task<string> PwdAsync(CancellationToken ct = default);
        Task<ShellCommandResult> RawAsync(string cmd, CancellationToken ct = default);
    }
}