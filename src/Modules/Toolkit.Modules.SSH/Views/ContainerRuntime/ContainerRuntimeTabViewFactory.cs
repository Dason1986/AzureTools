using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;
using Toolkit.Modules.SSH.ViewModels.ContainerRuntime;
using Toolkit.Modules.SSH.Views.ContainerRuntime.Tabs;

namespace Toolkit.Modules.SSH.Views.ContainerRuntime
{
    internal static class ContainerRuntimeTabViewFactory
    {
        private static readonly IReadOnlyDictionary<string, Func<IReadOnlyList<ContainerRuntimeTabViewModel>, IReadOnlyList<TabItem>>> TabBuilders =
            new Dictionary<string, Func<IReadOnlyList<ContainerRuntimeTabViewModel>, IReadOnlyList<TabItem>>>(StringComparer.OrdinalIgnoreCase)
            {
                [SshContainerEngine.Docker] = CreateSharedTabs,
                [SshContainerEngine.Podman] = CreatePodmanTabs
            };

        public static IReadOnlyList<TabItem> Create(string engine, IReadOnlyList<ContainerRuntimeTabViewModel> tabs)
        {
            Func<IReadOnlyList<ContainerRuntimeTabViewModel>, IReadOnlyList<TabItem>> builder =
                TabBuilders.TryGetValue(engine ?? string.Empty, out Func<IReadOnlyList<ContainerRuntimeTabViewModel>, IReadOnlyList<TabItem>> found)
                    ? found
                    : CreateSharedTabs;
            return builder(tabs);
        }

        private static IReadOnlyList<TabItem> CreateSharedTabs(IReadOnlyList<ContainerRuntimeTabViewModel> tabs)
        {
            return new[]
            {
                CreateTabItem(SSHResource.TabContainers, FindTab<ContainersTabViewModel>(tabs)),
                CreateTabItem(SSHResource.TabImages, FindTab<ImagesTabViewModel>(tabs)),
                CreateTabItem(SSHResource.TabVolumes, FindTab<VolumesTabViewModel>(tabs)),
                CreateTabItem(SSHResource.TabNetworks, FindTab<NetworksTabViewModel>(tabs))
            };
        }

        private static IReadOnlyList<TabItem> CreatePodmanTabs(IReadOnlyList<ContainerRuntimeTabViewModel> tabs)
        {
            PodsTabViewModel pods = tabs.OfType<PodsTabViewModel>().FirstOrDefault();
            if (pods == null) return CreateSharedTabs(tabs);
            List<TabItem> items = new List<TabItem>(CreateSharedTabs(tabs));
            items.Add(CreateTabItem(SSHResource.TabPods, pods));
            return items;
        }

        private static T FindTab<T>(IReadOnlyList<ContainerRuntimeTabViewModel> tabs) where T : ContainerRuntimeTabViewModel
        {
            return tabs.OfType<T>().First();
        }

        private static TabItem CreateTabItem(string header, ContainerRuntimeTabViewModel viewModel)
        {
            return new TabItem { Header = header, Content = CreateView(viewModel) };
        }

        private static UserControl CreateView(ContainerRuntimeTabViewModel viewModel)
        {
            return viewModel switch
            {
                ContainersTabViewModel containers => new ContainersTabView { DataContext = containers },
                ImagesTabViewModel images => new ImagesTabView { DataContext = images },
                VolumesTabViewModel volumes => new VolumesTabView { DataContext = volumes },
                NetworksTabViewModel networks => new NetworksTabView { DataContext = networks },
                PodsTabViewModel pods => new PodsTabView { DataContext = pods },
                _ => throw new ArgumentException($"Unknown container runtime tab view model: {viewModel.GetType()}")
            };
        }
    }
}
