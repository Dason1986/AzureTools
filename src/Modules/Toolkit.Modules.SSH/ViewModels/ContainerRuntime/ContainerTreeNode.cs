using System;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Modules.SSH.ContainerRuntime.Models;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    /// <summary>容器树形列表节点：一级为独立容器或 Pod/Compose 的 infra 容器，二级为该 Pod 下的容器。</summary>
    public class ContainerTreeNode
    {
        public ContainerTreeNode(Container container)
        {
            Container = container;
            Children = Array.Empty<ContainerTreeNode>();
        }

        public ContainerTreeNode(Container container, IEnumerable<Container> children)
        {
            Container = container;
            Children = children.Select(c => new ContainerTreeNode(c)).ToList();
        }

        public Container Container { get; }

        public IReadOnlyList<ContainerTreeNode> Children { get; }

        public string Name => Container.Name;

        public string Status => Container.Status;

        public string Image => Container.Image;

        public string State => Container.State;
    }
}
