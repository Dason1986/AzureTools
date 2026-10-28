namespace Toolkit.Modules.SSH
{
    public static class SshViewKeys
    {
        public const string DockerBrowser = "DockerBrowser";

        public const string PodmanBrowser = "PodmanBrowser";

        public const string PackageManager = "PackageManager";

        public const string FirewallManager = "FirewallManager";

        public static class PackageManagerKeys
        {
            public const string Host = "host";
        }

        public static class FirewallManagerKeys
        {
            public const string Host = "host";
        }

        public static class DockerBrowserParams
        {
            public const string Host = "host";
        }

        public static class TerminalKeys
        {
            public const string Key = "CliStreamSession";
            public const string Session = "session";
            public const string Title = "title";
            public const string IsInteractive = "isInteractive";
        }
    }
}
