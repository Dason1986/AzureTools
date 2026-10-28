using System;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.Models
{
    public static class SshContainerEngine
    {
        public const string Docker = "docker";
        public const string Podman = "podman";

        public static bool IsPodman(string engine)
        {
            return string.Equals(engine, Podman, StringComparison.OrdinalIgnoreCase);
        }

        public static string GetCliBinary(string engine)
        {
            return IsPodman(engine) ? "podman" : "docker";
        }

        public static string GetBrowserTitleFormat(string engine)
        {
            return IsPodman(engine) ? SSHResource.TitlePodmanBrowser : SSHResource.TitleDockerBrowser;
        }

        public static string GetLoadFailedMessage(string engine, string error)
        {
            return string.Format(IsPodman(engine) ? SSHResource.MsgPodmanLoadFailed : SSHResource.MsgDockerLoadFailed, error);
        }
    }
}
