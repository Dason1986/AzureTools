using Avalonia.Controls;
using System;
using Toolkit.Modules.SSH.ViewModels.SshHosts;

namespace Toolkit.Modules.SSH.Views.SshHosts
{
    internal static class FeatureTabViewFactory
    {
        public static UserControl CreateView(SshFeatureTabViewModel vm)
        {
            return vm switch
            {
                DockerFeatureTabViewModel docker => new DockerFeatureTabView { DataContext = docker },
                PodmanFeatureTabViewModel podman => new PodmanFeatureTabView { DataContext = podman },
                TerminalFeatureTabViewModel terminal => new TerminalFeatureTabView { DataContext = terminal },
                PackageFeatureTabViewModel package => new PackageFeatureTabView { DataContext = package },
                FirewallFeatureTabViewModel firewall => new FirewallFeatureTabView { DataContext = firewall },
                FileBrowserFeatureTabViewModel fileBrowser => new FileBrowserFeatureTabView { DataContext = fileBrowser },
                _ => throw new ArgumentException($"Unknown feature tab type: {vm.GetType()}")
            };
        }
    }
}
