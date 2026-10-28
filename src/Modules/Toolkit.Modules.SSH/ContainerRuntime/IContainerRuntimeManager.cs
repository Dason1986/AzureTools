using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime.Models;

namespace Toolkit.Modules.SSH.ContainerRuntime
{
    public interface IContainerRuntimeManager
    {
        Task<IReadOnlyList<Container>> ListContainersAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Image>> ListImagesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RegistryImage>> SearchImagesAsync(string keyword, CancellationToken ct = default);
        Task StartContainerAsync(string id, CancellationToken ct = default);
        Task StopContainerAsync(string id, CancellationToken ct = default);
        Task<string> GetContainerLogsAsync(string id, CancellationToken ct = default);
        Task<string> GetContainerDetailAsync(string id, CancellationToken ct = default);
        Task<ContainerStats> GetContainerStatsAsync(string id, CancellationToken ct = default);
        Task<string> GetImageDetailAsync(string id, CancellationToken ct = default);
        Task<ImageInspect> InspectImageAsync(string id, CancellationToken ct = default);
        Task<Container> CreateContainerAsync(ContainerCreateConfig config, string name = null, CancellationToken ct = default);
        Task RemoveContainerAsync(string id, bool force = false, CancellationToken ct = default);
        Task PruneContainersAsync(CancellationToken ct = default);
        Task RemoveImageAsync(string id, bool force = false, CancellationToken ct = default);
        Task PruneImagesAsync(CancellationToken ct = default);
        Task PullImageAsync(string image, CancellationToken ct = default);
        Task TagImageAsync(string id, string repositoryTag, CancellationToken ct = default);
        Task ExportImageAsync(string id, Stream destination, CancellationToken ct = default);
        Task ImportImageAsync(Stream source, CancellationToken ct = default);
        Task<IReadOnlyList<Volume>> ListVolumesAsync(CancellationToken ct = default);
        Task CreateVolumeAsync(string name, string driver = null, CancellationToken ct = default);
        Task RemoveVolumeAsync(string name, CancellationToken ct = default);
        Task PruneVolumesAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Network>> ListNetworksAsync(CancellationToken ct = default);
        Task CreateNetworkAsync(string name, string driver = null, CancellationToken ct = default);
        Task RemoveNetworkAsync(string id, CancellationToken ct = default);
        Task PruneNetworksAsync(CancellationToken ct = default);
        Task<IReadOnlyList<NetworkContainer>> GetNetworkContainersAsync(string networkId, CancellationToken ct = default);
        Task NetworkConnectAsync(string networkId, string containerId, CancellationToken ct = default);
        Task NetworkDisconnectAsync(string networkId, string containerId, CancellationToken ct = default);
    }
}
