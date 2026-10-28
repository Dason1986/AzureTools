using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.ContainerRuntime.Models.Http;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ContainerRuntime
{
    public sealed class HttpContainerRuntimeManager : IContainerRuntimeManager, IDisposable
    {
        private const string RemoteHost = "127.0.0.1";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan TransferTimeout = TimeSpan.FromHours(1);
        private readonly ISshTunnelManager _tunnel;
        private readonly SshHostProfile _host;
        private readonly HttpClient _http;
        private readonly HttpClient _transferHttp;
        private readonly int _remotePort;
        private int? _localPort;
        private bool _disposed;

        public HttpContainerRuntimeManager(ISshTunnelManager tunnel, SshHostProfile host, int remotePort)
        {
            _tunnel = tunnel ?? throw new ArgumentNullException(nameof(tunnel));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _remotePort = remotePort;
            _http = new HttpClient { Timeout = Timeout };
            _transferHttp = new HttpClient { Timeout = TransferTimeout };
        }

        public async Task<IReadOnlyList<Container>> ListContainersAsync(CancellationToken ct = default)
        {
            List<HttpContainer> items = await GetJsonAsync("v1.24/containers/json?all=true", SshJsonSerializerContext.Default.ListHttpContainer, ct).ConfigureAwait(false);
            return items.ConvertAll(MapContainer);
        }

        public async Task<IReadOnlyList<Image>> ListImagesAsync(CancellationToken ct = default)
        {
            List<HttpImage> items = await GetJsonAsync("v1.24/images/json", SshJsonSerializerContext.Default.ListHttpImage, ct).ConfigureAwait(false);
            return items.ConvertAll(MapImage);
        }

        public async Task<IReadOnlyList<RegistryImage>> SearchImagesAsync(string keyword, CancellationToken ct = default)
        {
            List<HttpRegistryImage> items = await GetJsonAsync($"v1.24/images/search?term={Uri.EscapeDataString(keyword)}&limit=25", SshJsonSerializerContext.Default.ListHttpRegistryImage, ct).ConfigureAwait(false);
            return items.ConvertAll(MapRegistryImage);
        }

        private static RegistryImage MapRegistryImage(HttpRegistryImage item)
        {
            return new RegistryImage
            {
                Name = item.Name,
                Description = item.Description,
                StarCount = item.StarCount,
                IsOfficial = item.IsOfficial
            };
        }

        public Task StartContainerAsync(string id, CancellationToken ct = default)
            => PostVoidAsync($"v1.24/containers/{Uri.EscapeDataString(id)}/start", ct);

        public Task StopContainerAsync(string id, CancellationToken ct = default)
            => PostVoidAsync($"v1.24/containers/{Uri.EscapeDataString(id)}/stop", ct);

        public async Task<string> GetContainerLogsAsync(string id, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.GetAsync($"v1.24/containers/{Uri.EscapeDataString(id)}/logs?stdout=1&stderr=1", ct).ConfigureAwait(false);
            r.EnsureSuccessStatusCode();
            using Stream s = await r.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            return await ReadDockerLogsAsync(s, ct).ConfigureAwait(false);
        }

        public async Task<string> GetContainerDetailAsync(string id, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.GetAsync($"v1.24/containers/{Uri.EscapeDataString(id)}/json", ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            return await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        public async Task<ContainerStats> GetContainerStatsAsync(string id, CancellationToken ct = default)
        {
            HttpContainerStats stats = await GetJsonAsync($"v1.24/containers/{Uri.EscapeDataString(id)}/stats?stream=false", SshJsonSerializerContext.Default.HttpContainerStats, ct).ConfigureAwait(false);
            return MapStats(stats);
        }

        private static ContainerStats MapStats(HttpContainerStats s)
        {
            double cpuPercent = 0;
            if (s?.CpuStats?.CpuUsage != null && s.PreCpuStats?.CpuUsage != null)
            {
                long cpuDelta = s.CpuStats.CpuUsage.TotalUsage - s.PreCpuStats.CpuUsage.TotalUsage;
                long sysDelta = s.CpuStats.SystemCpuUsage - s.PreCpuStats.SystemCpuUsage;
                if (sysDelta > 0 && cpuDelta > 0)
                    cpuPercent = (double)cpuDelta / sysDelta * 100.0;
            }
            long usage = s?.MemoryStats?.Usage ?? 0;
            long limit = s?.MemoryStats?.Limit ?? 0;
            return new ContainerStats
            {
                CpuPercent = cpuPercent,
                MemoryUsage = usage,
                MemoryLimit = limit,
                BlockReadBytes = SumBlockBytes(s, "Read"),
                BlockWriteBytes = SumBlockBytes(s, "Write"),
                NetworkReceivedBytes = SumNetworkBytes(s, true),
                NetworkTransmittedBytes = SumNetworkBytes(s, false)
            };
        }

        private static long SumBlockBytes(HttpContainerStats stats, string operation)
        {
            long total = 0;
            HttpBlkioEntry[] entries = stats?.BlkioStats?.IoServiceBytesRecursive;
            if (entries == null) return total;
            foreach (HttpBlkioEntry entry in entries)
            {
                if (string.Equals(entry.Op, operation, StringComparison.OrdinalIgnoreCase)) total += entry.Value;
            }
            return total;
        }

        private static long SumNetworkBytes(HttpContainerStats stats, bool received)
        {
            long total = 0;
            Dictionary<string, HttpNetworkStats> networks = stats?.Networks;
            if (networks == null) return total;
            foreach (HttpNetworkStats network in networks.Values)
            {
                total += received ? network.RxBytes : network.TxBytes;
            }
            return total;
        }

        public async Task<string> GetImageDetailAsync(string id, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.GetAsync($"v1.24/images/{Uri.EscapeDataString(id)}/json", ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            return await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        public async Task<ImageInspect> InspectImageAsync(string id, CancellationToken ct = default)
        {
            string json = await GetImageDetailAsync(id, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize(json, SshJsonSerializerContext.Default.ImageInspect);
        }

        public async Task<Container> CreateContainerAsync(ContainerCreateConfig config, string name = null, CancellationToken ct = default)
        {
            HttpContainerCreateRequest request = new HttpContainerCreateRequest
            {
                Image = config.Image,
                Cmd = config.Cmd,
                Env = config.Env,
                HostConfig = new HttpHostConfig
                {
                    Privileged = config.Privileged,
                    AutoRemove = config.AutoRemove,
                    RestartPolicy = string.IsNullOrEmpty(config.RestartPolicy) || config.RestartPolicy == "no"
                        ? null
                        : new HttpRestartPolicy { Name = config.RestartPolicy },
                    Binds = config.Binds,
                    PortBindings = BuildPortBindings(config.PortBindings)
                }
            };
            string body = JsonSerializer.Serialize(request, SshJsonSerializerContext.Default.HttpContainerCreateRequest);
            using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            string containerName = name ?? config.Name;
            string query = string.IsNullOrEmpty(containerName) ? string.Empty : $"?name={Uri.EscapeDataString(containerName)}";
            HttpCreateResponse resp = await PostJsonAsync($"v1.24/containers/create{query}", content, SshJsonSerializerContext.Default.HttpCreateResponse, ct).ConfigureAwait(false);
            return new Container { Id = resp?.Id };
        }

        private static Dictionary<string, List<HttpPortBinding>> BuildPortBindings(List<string> portBindings)
        {
            if (portBindings == null || portBindings.Count == 0) return null;
            Dictionary<string, List<HttpPortBinding>> result = new Dictionary<string, List<HttpPortBinding>>();
            foreach (string item in portBindings)
            {
                int idx = item.IndexOf(':');
                if (idx < 0) continue;
                string hostPort = item.Substring(0, idx);
                string containerPort = item.Substring(idx + 1);
                if (!containerPort.EndsWith("/tcp") && !containerPort.EndsWith("/udp"))
                    containerPort += "/tcp";
                if (!result.ContainsKey(containerPort))
                    result[containerPort] = new List<HttpPortBinding>();
                result[containerPort].Add(new HttpPortBinding { HostPort = hostPort });
            }
            return result.Count > 0 ? result : null;
        }

        public Task RemoveContainerAsync(string id, bool force = false, CancellationToken ct = default)
            => DeleteVoidAsync($"v1.24/containers/{Uri.EscapeDataString(id)}?force={force.ToString().ToLowerInvariant()}", ct);

        public Task PruneContainersAsync(CancellationToken ct = default)
            => PostVoidAsync("v1.24/containers/prune", ct);

        public Task RemoveImageAsync(string id, bool force = false, CancellationToken ct = default)
            => DeleteVoidAsync($"v1.24/images/{Uri.EscapeDataString(id)}?force={force.ToString().ToLowerInvariant()}", ct);

        public Task PruneImagesAsync(CancellationToken ct = default)
            => PostVoidAsync("v1.24/images/prune", ct);

        public async Task PullImageAsync(string image, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _transferHttp.PostAsync($"v1.24/images/create?fromImage={Uri.EscapeDataString(image)}", null, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        public async Task TagImageAsync(string id, string repositoryTag, CancellationToken ct = default)
        {
            (string repository, string tag) = SplitRepositoryTag(repositoryTag);
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            string query = $"?repo={Uri.EscapeDataString(repository)}&tag={Uri.EscapeDataString(tag)}";
            using HttpResponseMessage r = await _http.PostAsync($"v1.24/images/{Uri.EscapeDataString(id)}/tag{query}", null, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
        }

        public async Task ExportImageAsync(string id, Stream destination, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _transferHttp.GetAsync($"v1.24/images/{Uri.EscapeDataString(id)}/get", HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            using Stream source = await r.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            await source.CopyToAsync(destination, ct).ConfigureAwait(false);
            await destination.FlushAsync(ct).ConfigureAwait(false);
        }

        public async Task ImportImageAsync(Stream source, CancellationToken ct = default)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using StreamContent content = new StreamContent(source);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-tar");
            using HttpResponseMessage r = await _transferHttp.PostAsync("v1.24/images/load", content, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }

        private static (string Repository, string Tag) SplitRepositoryTag(string repositoryTag)
        {
            if (string.IsNullOrWhiteSpace(repositoryTag)) return (string.Empty, "latest");
            int slash = repositoryTag.LastIndexOf('/');
            int colon = repositoryTag.LastIndexOf(':');
            return colon > slash
                ? (repositoryTag[..colon], repositoryTag[(colon + 1)..])
                : (repositoryTag, "latest");
        }

        public async Task<IReadOnlyList<Volume>> ListVolumesAsync(CancellationToken ct = default)
        {
            HttpVolumeList resp = await GetJsonAsync("v1.24/volumes", SshJsonSerializerContext.Default.HttpVolumeList, ct).ConfigureAwait(false);
            return resp?.Volumes ?? new List<Volume>();
        }

        public Task CreateVolumeAsync(string name, string driver = null, CancellationToken ct = default)
        {
            string body = JsonSerializer.Serialize(new HttpVolumeCreateRequest { Name = name, Driver = driver }, SshJsonSerializerContext.Default.HttpVolumeCreateRequest);
            using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            return PostVoidAsync("v1.24/volumes/create", content, ct);
        }

        public Task RemoveVolumeAsync(string name, CancellationToken ct = default)
            => DeleteVoidAsync($"v1.24/volumes/{Uri.EscapeDataString(name)}", ct);

        public Task PruneVolumesAsync(CancellationToken ct = default)
            => PostVoidAsync("v1.41/volumes/prune", ct);

        public async Task<IReadOnlyList<Network>> ListNetworksAsync(CancellationToken ct = default)
            => await GetJsonAsync("v1.24/networks", SshJsonSerializerContext.Default.ListNetwork, ct).ConfigureAwait(false);

        public Task CreateNetworkAsync(string name, string driver = null, CancellationToken ct = default)
        {
            string body = JsonSerializer.Serialize(new HttpNetworkCreateRequest { Name = name, Driver = driver }, SshJsonSerializerContext.Default.HttpNetworkCreateRequest);
            using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            return PostVoidAsync("v1.24/networks/create", content, ct);
        }

        public Task RemoveNetworkAsync(string id, CancellationToken ct = default)
            => DeleteVoidAsync($"v1.24/networks/{Uri.EscapeDataString(id)}", ct);

        public Task PruneNetworksAsync(CancellationToken ct = default)
            => PostVoidAsync("v1.25/networks/prune", ct);

        public async Task<IReadOnlyList<NetworkContainer>> GetNetworkContainersAsync(string networkId, CancellationToken ct = default)
        {
            HttpNetworkInspect inspect = await GetJsonAsync($"v1.24/networks/{Uri.EscapeDataString(networkId)}", SshJsonSerializerContext.Default.HttpNetworkInspect, ct).ConfigureAwait(false);
            return MapNetworkContainers(inspect?.Containers);
        }

        public Task NetworkConnectAsync(string networkId, string containerId, CancellationToken ct = default)
            => PostNetworkContainerAsync("connect", networkId, containerId, ct);

        public Task NetworkDisconnectAsync(string networkId, string containerId, CancellationToken ct = default)
            => PostNetworkContainerAsync("disconnect", networkId, containerId, ct);

        private async Task PostNetworkContainerAsync(string action, string networkId, string containerId, CancellationToken ct)
        {
            string body = JsonSerializer.Serialize(new HttpNetworkConnectRequest { Container = containerId }, SshJsonSerializerContext.Default.HttpNetworkConnectRequest);
            using StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            await PostVoidAsync($"v1.24/networks/{Uri.EscapeDataString(networkId)}/{action}", content, ct).ConfigureAwait(false);
        }

        private static IReadOnlyList<NetworkContainer> MapNetworkContainers(Dictionary<string, HttpNetworkEndpoint> containers)
        {
            List<NetworkContainer> result = new List<NetworkContainer>();
            if (containers == null) return result;
            foreach (KeyValuePair<string, HttpNetworkEndpoint> pair in containers)
            {
                result.Add(new NetworkContainer { Id = pair.Key, Name = pair.Value?.Name, IpAddress = pair.Value?.IPv4Address });
            }
            return result;
        }

        private async Task EnsureTunnelAsync(CancellationToken ct)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpContainerRuntimeManager));
            if (_localPort.HasValue) return;
            int port = await _tunnel.OpenAsync(_host, RemoteHost, _remotePort, ct).ConfigureAwait(false);
            _localPort = port;
            Uri baseAddress = new Uri($"http://127.0.0.1:{port}");
            _http.BaseAddress = baseAddress;
            _transferHttp.BaseAddress = baseAddress;
        }

        private async Task<T> GetJsonAsync<T>(string relative, JsonTypeInfo<T> typeInfo, CancellationToken ct)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.GetAsync(relative, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            string body = await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize(body, typeInfo);
        }

        private async Task<T> PostJsonAsync<T>(string relative, HttpContent content, JsonTypeInfo<T> typeInfo, CancellationToken ct)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.PostAsync(relative, content, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
            string body = await r.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize(body, typeInfo);
        }

        private Task PostVoidAsync(string relative, CancellationToken ct)
            => PostVoidAsync(relative, null, ct);

        private async Task PostVoidAsync(string relative, HttpContent content, CancellationToken ct)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.PostAsync(relative, content, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
        }

        private async Task DeleteVoidAsync(string relative, CancellationToken ct)
        {
            await EnsureTunnelAsync(ct).ConfigureAwait(false);
            using HttpResponseMessage r = await _http.DeleteAsync(relative, ct).ConfigureAwait(false);
            await EnsureSuccessAsync(r).ConfigureAwait(false);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage r)
        {
            if (r.IsSuccessStatusCode) return;
            string msg = await r.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new InvalidOperationException($"HTTP {(int)r.StatusCode} {r.ReasonPhrase}: {msg}");
        }

        private static async Task<string> ReadDockerLogsAsync(Stream s, CancellationToken ct)
        {
            StringBuilder sb = new StringBuilder();
            byte[] header = new byte[8];
            while (true)
            {
                int read = 0;
                while (read < 8)
                {
                    int n = await s.ReadAsync(header.AsMemory(read, 8 - read), ct).ConfigureAwait(false);
                    if (n == 0) return sb.ToString();
                    read += n;
                }
                int len = (header[4] << 24) | (header[5] << 16) | (header[6] << 8) | header[7];
                if (len <= 0) continue;
                byte[] buf = new byte[len];
                int got = 0;
                while (got < len)
                {
                    int n = await s.ReadAsync(buf.AsMemory(got, len - got), ct).ConfigureAwait(false);
                    if (n == 0) break;
                    got += n;
                }
                sb.AppendLine(Encoding.UTF8.GetString(buf, 0, got));
            }
        }

        private static Container MapContainer(HttpContainer c)
        {
            return new Container
            {
                Id = c.Id,
                Name = c.Names != null && c.Names.Count > 0 ? c.Names[0].TrimStart('/') : string.Empty,
                Image = c.Image,
                State = c.State,
                Status = c.Status
            };
        }

        private static Image MapImage(HttpImage i)
        {
            string tag = i.RepoTags != null && i.RepoTags.Count > 0 ? i.RepoTags[0] : "<none>:<none>";
            string[] parts = tag.Split(':', 2);
            return new Image
            {
                Id = i.Id,
                Repository = parts.Length > 0 ? parts[0] : string.Empty,
                Tag = parts.Length > 1 ? parts[1] : string.Empty,
                Size = i.Size.ToString()
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _http.Dispose(); } catch { }
            try { _transferHttp.Dispose(); } catch { }
            if (_localPort.HasValue)
            {
                try { _ = _tunnel.CloseAsync(_host.Id, RemoteHost, _remotePort); } catch { }
            }
        }
    }
}
