using AzureTools.ViewModels;
using System;
using System.Collections.Generic;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public class SshHostEditorOption : EditorOption<SshHostProfile>
    {
        private static readonly IReadOnlyDictionary<string, Func<SshFeatureTabViewModel>> DefaultFeatureTabFactories = new Dictionary<string, Func<SshFeatureTabViewModel>>
        {
            [SshHostFeature.Docker] = () => new DockerFeatureTabViewModel(),
            [SshHostFeature.Podman] = () => new PodmanFeatureTabViewModel(),
            [SshHostFeature.Terminal] = () => new TerminalFeatureTabViewModel(),
            [SshHostFeature.Package] = () => new PackageFeatureTabViewModel(),
            [SshHostFeature.Firewall] = () => new FirewallFeatureTabViewModel(),
            [SshHostFeature.FileBrowser] = () => new FileBrowserFeatureTabViewModel()
        };

        public IReadOnlyDictionary<string, Func<SshFeatureTabViewModel>> FeatureTabFactories { get; set; } = DefaultFeatureTabFactories;
    }
}
