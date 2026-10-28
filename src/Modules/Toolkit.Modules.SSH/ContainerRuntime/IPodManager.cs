using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime.Models;

namespace Toolkit.Modules.SSH.ContainerRuntime
{
    /// <summary>
    /// Podman 独有的 Pod 管理能力，仅由 Podman 引擎实现。
    /// </summary>
    public interface IPodManager
    {
        Task<IReadOnlyList<Pod>> ListPodsAsync(CancellationToken ct = default);

        /// <summary>列出指定 Pod 下的所有容器（含已停止）。</summary>
        Task<IReadOnlyList<Container>> ListPodContainersAsync(string podName, CancellationToken ct = default);
        Task StartPodAsync(string podId, CancellationToken ct = default);
        Task StopPodAsync(string podId, CancellationToken ct = default);
        Task RemovePodAsync(string podId, bool force = false, CancellationToken ct = default);
        Task PrunePodsAsync(CancellationToken ct = default);

        /// <summary>创建一个空的 Pod。</summary>
        Task CreatePodAsync(string name, CancellationToken ct = default);

        /// <summary>上传 YAML 到远程临时文件并执行 podman kube play 部署。</summary>
        Task<string> PlayKubeYamlAsync(string yaml, CancellationToken ct = default);

        /// <summary>在指定 Pod 中以后台方式按完整配置创建容器。</summary>
        Task<Container> CreateContainerInPodAsync(string podName, ContainerCreateConfig config, string containerName = null, CancellationToken ct = default);
    }
}
