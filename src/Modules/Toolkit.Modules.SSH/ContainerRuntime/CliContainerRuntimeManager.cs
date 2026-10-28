using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public class PodmanCliContainerRuntimeManager : CliContainerRuntimeManager, IPodManager
    {
        public PodmanCliContainerRuntimeManager(ISshConnectionManager connectionManager, SshHostProfile host) : base(connectionManager, host, SshContainerEngine.Podman)
        {
        }

        public async Task<IReadOnlyList<Pod>> ListPodsAsync(CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("pod ps --format '{{json .}}'"), ct).ConfigureAwait(false);
            return ParseLines<Pod>(output, SshJsonSerializerContext.Default.Pod);
        }

        public async Task<IReadOnlyList<Container>> ListPodContainersAsync(string podName, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd($"ps -a --filter pod={ShellQuote(podName)} --format json"), ct).ConfigureAwait(false);
            List<Container> containers = JsonSerializer.Deserialize(output, SshJsonSerializerContext.Default.ListContainer);
            return containers?.Where(c => !c.IsInfra).ToList() ?? new List<Container>();
        }

        public Task StartPodAsync(string podId, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"pod start {ShellQuote(podId)}"), ct);

        public Task StopPodAsync(string podId, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"pod stop {ShellQuote(podId)}"), ct);

        public Task RemovePodAsync(string podId, bool force = false, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"pod rm {(force ? "-f " : string.Empty)}{ShellQuote(podId)}"), ct);

        public Task PrunePodsAsync(CancellationToken ct = default)
            => RunVoidAsync(Cmd("pod prune -f"), ct);

        public Task CreatePodAsync(string name, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"pod create --name {ShellQuote(name)}"), ct);

        public async Task<string> PlayKubeYamlAsync(string yaml, CancellationToken ct = default)
        {
            string remotePath = CreateRemoteTempPath("yaml");
            try
            {
                using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
                await UploadAsync(stream, remotePath, ct).ConfigureAwait(false);
                return await RunAsync(Cmd($"kube play {ShellQuote(remotePath)}"), ct, LongCommandTimeout).ConfigureAwait(false);
            }
            finally
            {
                await RemoveRemoteTempFileAsync(remotePath).ConfigureAwait(false);
            }
        }

        public async Task<Container> CreateContainerInPodAsync(string podName, ContainerCreateConfig config, string containerName = null, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd($"run -d --pod {ShellQuote(podName)}" + BuildContainerArgs(config, containerName)), ct, LongCommandTimeout).ConfigureAwait(false);
            return new Container { Id = output.Trim() };
        }
    }
    public class DockerCliContainerRuntimeManager : CliContainerRuntimeManager
    {
        public DockerCliContainerRuntimeManager(ISshConnectionManager connectionManager, SshHostProfile host) : base(connectionManager, host, SshContainerEngine.Docker)
        {
        }
    }
    public abstract class CliContainerRuntimeManager : IContainerRuntimeManager
    {
        private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(60);
        protected static readonly TimeSpan LongCommandTimeout = TimeSpan.FromMinutes(10);
        private readonly ISshConnectionManager _connectionManager;
        private readonly SshHostProfile _host;
        private readonly string _binary;

        public CliContainerRuntimeManager(ISshConnectionManager connectionManager, SshHostProfile host, string binaryName)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _binary = binaryName;
        }

        public async Task<IReadOnlyList<Container>> ListContainersAsync(CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("ps -a --format '{{json .}}'"), ct).ConfigureAwait(false);
            return ParseLines<Container>(output, SshJsonSerializerContext.Default.Container);
        }

        public async Task<IReadOnlyList<Image>> ListImagesAsync(CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("images --format '{{json .}}'"), ct).ConfigureAwait(false);
            return ParseLines<Image>(output, SshJsonSerializerContext.Default.Image);
        }

        public async Task<IReadOnlyList<RegistryImage>> SearchImagesAsync(string keyword, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd($"search {ShellQuote(keyword)} --limit 10 --format '{{{{json .}}}}'"), ct).ConfigureAwait(false);
            return ParseRegistryImages(output);
        }

        public Task StartContainerAsync(string id, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"start {ShellQuote(id)}"), ct);

        public Task StopContainerAsync(string id, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"stop {ShellQuote(id)}"), ct);

        public async Task<string> GetContainerLogsAsync(string id, CancellationToken ct = default)
            => await RunAsync(Cmd($"logs --tail 500 {ShellQuote(id)} 2>&1"), ct).ConfigureAwait(false);

        public Task<string> GetContainerDetailAsync(string id, CancellationToken ct = default)
            => RunAsync(Cmd($"inspect {ShellQuote(id)}"), ct);

        public async Task<ContainerStats> GetContainerStatsAsync(string id, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd($"stats --no-stream --format json {ShellQuote(id)}"), ct).ConfigureAwait(false);
            return ParseCliStats(output.Trim());
        }

        private static ContainerStats ParseCliStats(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;
            double cpuPercent = root.TryGetProperty("CPUPerc", out JsonElement cpuEl) && double.TryParse(cpuEl.GetString()?.TrimEnd('%'), out double cpu) ? cpu : 0;
            long memoryUsage = 0, memoryLimit = 0;
            if (root.TryGetProperty("MemUsage", out JsonElement memEl))
            {
                string[] parts = memEl.GetString().Split('/');
                if (parts.Length >= 2)
                {
                    memoryUsage = ParseSize(parts[0].Trim());
                    memoryLimit = ParseSize(parts[1].Trim());
                }
            }

            ParseIoPair(root, "BlockIO", out long blockRead, out long blockWrite);
            ParseIoPair(root, "NetIO", out long netReceived, out long netTransmitted);

            return new ContainerStats
            {
                CpuPercent = cpuPercent,
                MemoryUsage = memoryUsage,
                MemoryLimit = memoryLimit,
                BlockReadBytes = blockRead,
                BlockWriteBytes = blockWrite,
                NetworkReceivedBytes = netReceived,
                NetworkTransmittedBytes = netTransmitted
            };
        }

        private static void ParseIoPair(JsonElement root, string propertyName, out long first, out long second)
        {
            first = 0;
            second = 0;
            if (!root.TryGetProperty(propertyName, out JsonElement element)) return;
            string[] parts = element.GetString()?.Split('/') ?? Array.Empty<string>();
            if (parts.Length < 2) return;
            first = ParseSize(parts[0]);
            second = ParseSize(parts[1]);
        }

        private static long ParseSize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            string trimmed = text.Trim();
            string number = new string(trimmed.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            string unit = trimmed[number.Length..].Trim().ToUpperInvariant();
            if (!double.TryParse(number, out double value)) return 0;
            return unit switch
            {
                "B" => (long)value,
                "KB" or "KIB" => (long)(value * 1024),
                "MB" or "MIB" => (long)(value * 1024 * 1024),
                "GB" or "GIB" => (long)(value * 1024 * 1024 * 1024),
                "TB" or "TIB" => (long)(value * 1024L * 1024 * 1024 * 1024),
                _ => (long)value
            };
        }

        public Task<string> GetImageDetailAsync(string id, CancellationToken ct = default)
            => RunAsync(Cmd($"inspect {ShellQuote(id)}"), ct);

        public async Task<ImageInspect> InspectImageAsync(string id, CancellationToken ct = default)
        {
            string output = await GetImageDetailAsync(id, ct).ConfigureAwait(false);
            List<ImageInspect> list = JsonSerializer.Deserialize(output, SshJsonSerializerContext.Default.ListImageInspect);
            return list != null && list.Count > 0 ? list[0] : null;
        }

        public async Task<Container> CreateContainerAsync(ContainerCreateConfig config, string name = null, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("create" + BuildContainerArgs(config, name)), ct).ConfigureAwait(false);
            return new Container { Id = output.Trim() };
        }

        protected string BuildContainerArgs(ContainerCreateConfig config, string name)
        {
            StringBuilder sb = new StringBuilder();
            string containerName = name ?? config.Name;
            if (!string.IsNullOrEmpty(containerName)) sb.Append(" --name ").Append(ShellQuote(containerName));
            if (config.Privileged) sb.Append(" --privileged");
            if (config.AutoRemove) sb.Append(" --rm");
            if (!string.IsNullOrEmpty(config.RestartPolicy) && config.RestartPolicy != "no") sb.Append(" --restart ").Append(ShellQuote(config.RestartPolicy));
            if (config.Networks != null)
            {
                foreach (string item in config.Networks) sb.Append(" --network ").Append(ShellQuote(item));
            }
            if (config.Env != null)
            {
                foreach (string item in config.Env) sb.Append(" -e ").Append(ShellQuote(item));
            }
            if (config.Binds != null)
            {
                foreach (string item in config.Binds) sb.Append(" -v ").Append(ShellQuote(item));
            }
            if (config.PortBindings != null)
            {
                foreach (string item in config.PortBindings) sb.Append(" -p ").Append(ShellQuote(item));
            }
            sb.Append(' ').Append(ShellQuote(config.Image));
            if (config.Cmd != null)
            {
                foreach (string arg in config.Cmd) sb.Append(' ').Append(ShellQuote(arg));
            }
            return sb.ToString();
        }

        public Task RemoveContainerAsync(string id, bool force = false, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"rm {(force ? "-f " : string.Empty)}{ShellQuote(id)}"), ct);

        public Task PruneContainersAsync(CancellationToken ct = default)
            => RunVoidAsync(Cmd("container prune -f"), ct);

        public Task RemoveImageAsync(string id, bool force = false, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"rmi {(force ? "-f " : string.Empty)}{ShellQuote(id)}"), ct);

        public Task PruneImagesAsync(CancellationToken ct = default)
            => RunVoidAsync(Cmd("image prune -f"), ct);

        public Task PullImageAsync(string image, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"pull {ShellQuote(image)}"), ct, LongCommandTimeout);

        public Task TagImageAsync(string id, string repositoryTag, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"tag {ShellQuote(id)} {ShellQuote(repositoryTag)}"), ct);

        public async Task ExportImageAsync(string id, Stream destination, CancellationToken ct = default)
        {
            string remotePath = CreateRemoteTempPath("tar");
            try
            {
                await RunVoidAsync(Cmd($"save -o {ShellQuote(remotePath)} {ShellQuote(id)}"), ct, LongCommandTimeout).ConfigureAwait(false);
                await DownloadAsync(remotePath, destination, ct).ConfigureAwait(false);
            }
            finally
            {
                await RemoveRemoteTempFileAsync(remotePath).ConfigureAwait(false);
            }
        }

        public async Task ImportImageAsync(Stream source, CancellationToken ct = default)
        {
            string remotePath = CreateRemoteTempPath("tar");
            try
            {
                await UploadAsync(source, remotePath, ct).ConfigureAwait(false);
                await RunVoidAsync(Cmd($"load -i {ShellQuote(remotePath)}"), ct, LongCommandTimeout).ConfigureAwait(false);
            }
            finally
            {
                await RemoveRemoteTempFileAsync(remotePath).ConfigureAwait(false);
            }
        }

        public async Task<IReadOnlyList<Volume>> ListVolumesAsync(CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("volume ls --format '{{json .}}'"), ct).ConfigureAwait(false);
            return ParseLines<Volume>(output, SshJsonSerializerContext.Default.Volume);
        }

        public Task CreateVolumeAsync(string name, string driver = null, CancellationToken ct = default)
        {
            StringBuilder sb = new StringBuilder(Cmd("volume create"));
            if (!string.IsNullOrEmpty(driver)) sb.Append(" --driver ").Append(ShellQuote(driver));
            sb.Append(' ').Append(ShellQuote(name));
            return RunVoidAsync(sb.ToString(), ct);
        }

        public Task RemoveVolumeAsync(string name, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"volume rm {ShellQuote(name)}"), ct);

        public Task PruneVolumesAsync(CancellationToken ct = default)
            => RunVoidAsync(Cmd("volume prune -f"), ct);

        public async Task<IReadOnlyList<Network>> ListNetworksAsync(CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd("network ls --format '{{json .}}'"), ct).ConfigureAwait(false);
            return ParseLines<Network>(output, SshJsonSerializerContext.Default.Network);
        }

        public Task CreateNetworkAsync(string name, string driver = null, CancellationToken ct = default)
        {
            StringBuilder sb = new StringBuilder(Cmd("network create"));
            if (!string.IsNullOrEmpty(driver)) sb.Append(" --driver ").Append(ShellQuote(driver));
            sb.Append(' ').Append(ShellQuote(name));
            return RunVoidAsync(sb.ToString(), ct);
        }

        public Task RemoveNetworkAsync(string id, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"network rm {ShellQuote(id)}"), ct);

        public Task PruneNetworksAsync(CancellationToken ct = default)
            => RunVoidAsync(Cmd("network prune -f"), ct);

        public async Task<IReadOnlyList<NetworkContainer>> GetNetworkContainersAsync(string networkId, CancellationToken ct = default)
        {
            string output = await RunAsync(Cmd($"network inspect {ShellQuote(networkId)}"), ct).ConfigureAwait(false);
            List<HttpNetworkInspect> items = JsonSerializer.Deserialize(output, SshJsonSerializerContext.Default.ListHttpNetworkInspect);
            return MapNetworkContainers(items != null && items.Count > 0 ? items[0].Containers : null);
        }

        public Task NetworkConnectAsync(string networkId, string containerId, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"network connect {ShellQuote(networkId)} {ShellQuote(containerId)}"), ct);

        public Task NetworkDisconnectAsync(string networkId, string containerId, CancellationToken ct = default)
            => RunVoidAsync(Cmd($"network disconnect {ShellQuote(networkId)} {ShellQuote(containerId)}"), ct);

        protected string Cmd(string arguments)
            => $"{_binary} {arguments}";

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

        protected async Task<string> RunAsync(string command, CancellationToken ct, TimeSpan? timeout = null)
        {
            SshClient client = _connectionManager.GetClient(_host);
            TimeSpan commandTimeout = timeout ?? CommandTimeout;
            return await Task.Run(() =>
            {
                using SshCommand cmd = client.CreateCommand(command);
                cmd.CommandTimeout = commandTimeout;
                cmd.Execute();
                if (cmd.ExitStatus != 0) throw new InvalidOperationException(cmd.Error);
                return cmd.Result;
            }, ct).ConfigureAwait(false);
        }

        protected async Task RunVoidAsync(string command, CancellationToken ct, TimeSpan? timeout = null)
            => await RunAsync(command, ct, timeout).ConfigureAwait(false);

        protected string CreateRemoteTempPath(string extension)
        {
            string workingDirectory = _connectionManager.GetSftpClient(_host).WorkingDirectory;
            return $"{workingDirectory.TrimEnd('/')}/azuretools-{Guid.NewGuid():N}.{extension}";
        }

        protected Task DownloadAsync(string remotePath, Stream destination, CancellationToken ct)
            => Task.Run(() =>
            {
                SftpClient client = _connectionManager.GetSftpClient(_host);
                lock (client) client.DownloadFile(remotePath, destination);
            }, ct);

        protected Task UploadAsync(Stream source, string remotePath, CancellationToken ct)
            => Task.Run(() =>
            {
                SftpClient client = _connectionManager.GetSftpClient(_host);
                lock (client) client.UploadFile(source, remotePath, true);
            }, ct);

        protected async Task RemoveRemoteTempFileAsync(string remotePath)
        {
            try
            {
                await RunVoidAsync(Cmd($"rm -f {ShellQuote(remotePath)}"), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        protected static List<T> ParseLines<T>(string output, JsonTypeInfo<T> typeInfo)
        {
            List<T> list = new List<T>();
            foreach (string line in output.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;
                list.Add(JsonSerializer.Deserialize(trimmed, typeInfo));
            }
            return list;
        }

        private static List<RegistryImage> ParseRegistryImages(string output)
        {
            List<RegistryImage> images = new List<RegistryImage>();
            foreach (string line in output.Split('\n'))
            {
                string json = line.Trim();
                if (json.Length == 0) continue;
                using JsonDocument document = JsonDocument.Parse(json);
                images.Add(MapRegistryImage(document.RootElement));
            }
            return images;
        }

        private static RegistryImage MapRegistryImage(JsonElement item)
        {
            return new RegistryImage
            {
                Name = ReadText(item, "Name", "name"),
                Description = ReadText(item, "Description", "description"),
                StarCount = ReadNumber(item, "StarCount", "Stars", "star_count", "stars"),
                IsOfficial = ReadFlag(item, "IsOfficial", "Official", "is_official", "official")
            };
        }

        private static string ReadText(JsonElement item, params string[] names)
        {
            foreach (string name in names)
            {
                if (item.TryGetProperty(name, out JsonElement value)) return value.ToString();
            }
            return null;
        }

        private static int ReadNumber(JsonElement item, params string[] names)
        {
            foreach (string name in names)
            {
                if (!item.TryGetProperty(name, out JsonElement value)) continue;
                return int.TryParse(value.ToString(), out int number) ? number : 0;
            }
            return 0;
        }

        private static bool ReadFlag(JsonElement item, params string[] names)
        {
            foreach (string name in names)
            {
                if (!item.TryGetProperty(name, out JsonElement value)) continue;
                string text = value.ToString();
                return bool.TryParse(text, out bool flag) ? flag : text.Contains("OK", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        protected static string ShellQuote(string value)
            => "'" + value.Replace("'", "'\\''") + "'";
    }
}
