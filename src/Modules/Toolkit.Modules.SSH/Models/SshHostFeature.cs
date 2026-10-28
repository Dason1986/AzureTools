using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.Models
{
    public class SshHostFeature
    {
        public const string FileBrowser = "FileBrowser";
        public const string Terminal = "Terminal";
        public const string Package = "Package";
        public const string Firewall = "Firewall";
        public const string Docker = SshContainerEngine.Docker;
        public const string Podman = SshContainerEngine.Podman;

        private const string FolderIcon = "M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z";
        private const string TerminalIcon = "M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2z M4 8h16v10H4z M8 15l4-3-4-3v6z M14 13h4v2h-4z";
        private const string PackageIcon = "M20 2H4c-1 0-2 .9-2 2v3.01c0 .72.43 1.34 1 1.69V20c0 1.1 1.1 2 2 2h14c.9 0 2-.9 2-2V8.7c.57-.35 1-.97 1-1.69V4c0-1.1-1-2-2-2zm-5 12H9v-2h6v2zm5-7H4V4h16v3z";
        private const string FirewallIcon = "M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm-2 16l-4-4 1.41-1.41L10 14.17l6.59-6.59L18 9l-8 8z";
        private const string ContainerGridIcon = "M3 3v8h8V3H3zm10 0v8h8V3h-8zM3 13v8h8v-8H3zm10 0v8h8v-8h-8z";
        private const string PodIcon = "M13 13v8h8v-8h-8zM3 21h8v-8H3v8zM3 3v8h8V3H3zm13.66-1.31L11 7.34 16.66 13l5.66-5.66-5.66-5.65z";

        public string Code { get; }

        public string Name { get; }

        public string Icon { get; }

        public SshHostFeature(string code, string name, string icon)
        {
            Code = code;
            Name = name;
            Icon = icon;
        }

        public static SshHostFeature[] All { get; } = new[]
        {
            new SshHostFeature(FileBrowser, SSHResource.FeatureFileBrowser, FolderIcon),
            new SshHostFeature(Terminal, SSHResource.FeatureTerminal, TerminalIcon),
            new SshHostFeature(Package, SSHResource.FeaturePackage, PackageIcon),
            new SshHostFeature(Firewall, SSHResource.FeatureFirewall, FirewallIcon),
            new SshHostFeature(Docker, SSHResource.FeatureDocker, ContainerGridIcon),
            new SshHostFeature(Podman, SSHResource.FeaturePodman, PodIcon)
        };
    }
}
