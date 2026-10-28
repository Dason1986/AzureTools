using System;
using System.Collections.Generic;
using Toolkit.Modules.SSH.ContainerRuntime.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    /// <summary>Pod 树形列表节点：一级为 Pod，二级为 Pod 下的容器。</summary>
    public class PodTreeNode
    {
        public PodTreeNode(Pod pod, IReadOnlyList<PodTreeNode> containers)
        {
            IsPod = true;
            Pod = pod;
            Name = pod.Name;
            Status = pod.Status;
            Detail = string.Format(Properties.SSHResource.MsgPodContainerCount, pod.NumContainers);
            Children = containers;
        }

        public PodTreeNode(Container container)
        {
            IsPod = false;
            Container = container;
            Name = container.Name ?? container.Id;
            Status = container.Status;
            Detail = container.Image;
            Children = Array.Empty<PodTreeNode>();
        }

        public bool IsPod { get; }

        public Pod Pod { get; }

        public Container Container { get; }

        public string Name { get; }

        public string Status { get; }

        public string Detail { get; }

        public IReadOnlyList<PodTreeNode> Children { get; }
    }
}
