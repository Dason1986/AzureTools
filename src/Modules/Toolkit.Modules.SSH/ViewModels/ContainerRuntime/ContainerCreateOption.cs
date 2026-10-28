using AzureTools.ViewModels;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public class ContainerCreateOption : IViewModelOption
    {
        public string Engine { get; set; } = SshContainerEngine.Docker;
        public string SelectedImage { get; set; }

        /// <summary>目标 Pod 名称，非空时容器创建到该 Pod 中（仅 Podman）。</summary>
        public string PodName { get; set; }

        /// <summary>为 true 时仅收集容器配置不执行创建，配置通过 CollectedConfig 返回。</summary>
        public bool CollectOnly { get; set; }

        /// <summary>编辑已有配置时传入，用于预填表单。</summary>
        public ContainerCreateConfig InitialConfig { get; set; }
    }
}
