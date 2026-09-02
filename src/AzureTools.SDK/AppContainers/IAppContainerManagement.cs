using AzureTools.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.SDK.AppContainers
{
    public interface IAppContainerManagement
    {
        string EnvironmentName { get; }
        bool SupportEnterContainer { get; }
        bool SupportContainerLogs { get; }

        Task<ContainerAppInfo[]> GetContainerAppsAsync(CancellationToken ct = default);

        Task<ContainerAppDetail> GetContainerAppDetailAsync(string containerAppName, CancellationToken ct = default);
        Task<ICliStreamSession> GetContainerLogsFollowAsync(string containerAppName, string resourceGroup, string revision, string replica, CancellationToken ct = default);
        Task<ICliStreamSession> ExecContainerAsync(string containerAppName, string resourceGroup, string revision, string replica, string containerName, CancellationToken ct = default);
        Task<IContainerFileProvider> GetContainerFileBrowserAsync(string containerAppName, string resourceGroup, string revision, string replica, string containerName, CancellationToken ct = default);
        Task RevisionRestartAsync(string containerAppName, string resourceGroup, string revision, CancellationToken ct = default);
    }
}
