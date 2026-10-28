using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolkit.Modules.SSH.Models
{
    public class PackageManagerCatalog
    {
        public const string Apt = "apt";
        public const string Dnf = "dnf";
        public const string Yum = "yum";
        public const string Apk = "apk";
        public const string Zypper = "zypper";
        public const string Pacman = "pacman";

        private static readonly string[] DnfArchSuffixes = { "x86_64", "noarch", "i686", "i386", "aarch64", "armv7hl", "s390x", "ppc64le", "riscv64", "x86" };

        public string Kind { get; }

        public string Binary { get; }

        public string SearchCommandFormat { get; }

        public string InstalledCommand { get; }

        public string InstallCommandFormat { get; }

        public string RemoveCommandFormat { get; }

        public string UpgradeCommandFormat { get; }

        public string DowngradeCommandFormat { get; }

        public bool DowngradeRequiresVersion { get; }

        public Func<string, IReadOnlyList<PackageInfo>> SearchParser { get; }

        public Func<string, IReadOnlyList<PackageInfo>> InstalledParser { get; }

        private PackageManagerCatalog(string kind, string binary, string searchCommandFormat, string installedCommand, string installCommandFormat, string removeCommandFormat, string upgradeCommandFormat, string downgradeCommandFormat, bool downgradeRequiresVersion, Func<string, IReadOnlyList<PackageInfo>> searchParser, Func<string, IReadOnlyList<PackageInfo>> installedParser)
        {
            Kind = kind;
            Binary = binary;
            SearchCommandFormat = searchCommandFormat;
            InstalledCommand = installedCommand;
            InstallCommandFormat = installCommandFormat;
            RemoveCommandFormat = removeCommandFormat;
            UpgradeCommandFormat = upgradeCommandFormat;
            DowngradeCommandFormat = downgradeCommandFormat;
            DowngradeRequiresVersion = downgradeRequiresVersion;
            SearchParser = searchParser;
            InstalledParser = installedParser;
        }

        public static PackageManagerCatalog Find(string kind)
            => All.FirstOrDefault(c => c.Kind == kind);

        public static PackageManagerCatalog Resolve(string osId, string osVersion)
        {
            if (string.IsNullOrWhiteSpace(osId)) return null;
            string id = osId.Trim().ToLowerInvariant();
            if (IsDebianFamily(id)) return Find(Apt);
            if (id == "alpine") return Find(Apk);
            if (IsSuseFamily(id)) return Find(Zypper);
            if (IsArchFamily(id)) return Find(Pacman);
            int major = ParseMajor(osVersion);
            switch (id)
            {
                case "fedora":
                case "rocky":
                case "almalinux":
                case "ol":
                    return Find(Dnf);
                case "amzn":
                    return major > 0 && major < 2023 ? Find(Yum) : Find(Dnf);
                case "centos":
                case "rhel":
                    return major > 0 && major < 8 ? Find(Yum) : Find(Dnf);
                case "scientific":
                    return major > 0 && major < 8 ? Find(Yum) : Find(Dnf);
                default:
                    return null;
            }
        }

        private static bool IsDebianFamily(string osId)
        {
            string[] ids = { "ubuntu", "debian", "linuxmint", "mint", "pop", "kali", "raspbian", "deepin", "uos", "neon", "elementary" };
            return ids.Contains(osId);
        }

        private static bool IsSuseFamily(string osId)
        {
            return osId.StartsWith("opensuse") || osId == "sles" || osId == "suse";
        }

        private static bool IsArchFamily(string osId)
        {
            string[] ids = { "arch", "manjaro", "endeavouros", "garuda", "arcolinux" };
            return ids.Contains(osId);
        }

        private static int ParseMajor(string osVersion)
        {
            if (string.IsNullOrWhiteSpace(osVersion)) return 0;
            string number = new string(osVersion.Trim().TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            string major = number.Split('.')[0];
            return int.TryParse(major, out int value) ? value : 0;
        }

        public static readonly PackageManagerCatalog[] All =
        {
            new PackageManagerCatalog(
                Apt, "apt-get",
                "apt-cache search {0}",
                "dpkg -l",
                "DEBIAN_FRONTEND=noninteractive apt-get install -y {0}",
                "DEBIAN_FRONTEND=noninteractive apt-get remove -y {0}",
                "DEBIAN_FRONTEND=noninteractive apt-get install --only-upgrade -y {0}",
                "DEBIAN_FRONTEND=noninteractive apt-get install -y --allow-downgrades {0}={1}",
                true,
                ParseAptSearch, ParseDpkgInstalled),
            new PackageManagerCatalog(
                Dnf, "dnf",
                "dnf -q search {0}",
                "dnf -q list installed",
                "dnf -q install -y {0}",
                "dnf -q remove -y {0}",
                "dnf -q upgrade -y {0}",
                "dnf -q downgrade -y {0}",
                false,
                ParseDnfSearch, ParseDnfInstalled),
            new PackageManagerCatalog(
                Yum, "yum",
                "yum -q search {0}",
                "yum -q list installed",
                "yum -q install -y {0}",
                "yum -q remove -y {0}",
                "yum -q upgrade -y {0}",
                "yum -q downgrade -y {0}",
                false,
                ParseDnfSearch, ParseDnfInstalled),
            new PackageManagerCatalog(
                Apk, "apk",
                "apk search -v {0}",
                "apk info -v",
                "apk add {0}",
                "apk del {0}",
                "apk add --upgrade {0}",
                "apk add {0}={1}",
                true,
                ParseApkSearch, ParseApkInstalled),
            new PackageManagerCatalog(
                Zypper, "zypper",
                "zypper -q search -s {0}",
                "zypper -q search -s -i",
                "zypper --non-interactive install {0}",
                "zypper --non-interactive remove {0}",
                "zypper --non-interactive install {0}",
                "zypper --non-interactive install --oldpackage {0}={1}",
                true,
                ParseZypperTable, ParseZypperTable),
            new PackageManagerCatalog(
                Pacman, "pacman",
                "pacman -Ss {0}",
                "pacman -Q",
                "pacman -S --noconfirm {0}",
                "pacman -R --noconfirm {0}",
                "pacman -S --noconfirm {0}",
                null,
                false,
                ParsePacmanSearch, ParsePacmanInstalled)
        };

        private static IReadOnlyList<PackageInfo> ParseAptSearch(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                int index = line.IndexOf(" - ", StringComparison.Ordinal);
                if (index <= 0) continue;
                list.Add(new PackageInfo { Name = line[..index].Trim(), Description = line[(index + 3)..].Trim() });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParseDpkgInstalled(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                if (!line.StartsWith("ii", StringComparison.Ordinal)) continue;
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;
                list.Add(new PackageInfo { Name = StripDpkgArch(parts[1]), Version = parts[2], Description = parts.Length > 3 ? string.Join(" ", parts[3..]) : null, IsInstalled = true });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParseDnfSearch(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                int index = line.IndexOf(" : ", StringComparison.Ordinal);
                if (index <= 0) continue;
                string name = StripArch(line[..index].Trim());
                if (name.Contains(' ')) continue;
                list.Add(new PackageInfo { Name = name, Description = line[(index + 3)..].Trim() });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParseDnfInstalled(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                if (line.Contains("Installed Packages") || line.Length == 0) continue;
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                list.Add(new PackageInfo { Name = StripArch(parts[0]), Version = parts[1], IsInstalled = true });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParseApkSearch(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                int index = line.IndexOf(" - ", StringComparison.Ordinal);
                string pkgVersion = index > 0 ? line[..index].Trim() : line.Trim();
                int separator = IndexOfVersionSeparator(pkgVersion);
                if (separator <= 0) continue;
                list.Add(new PackageInfo { Name = pkgVersion[..separator], Version = pkgVersion[(separator + 1)..], Description = index > 0 ? line[(index + 3)..].Trim() : null });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParseApkInstalled(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                int separator = IndexOfVersionSeparator(line);
                if (separator <= 0) continue;
                list.Add(new PackageInfo { Name = line[..separator], Version = line[(separator + 1)..], IsInstalled = true });
            }
            return list;
        }

        private static int IndexOfVersionSeparator(string text)
        {
            for (int i = 1; i < text.Length - 1; i++)
            {
                if (text[i] == '-' && char.IsDigit(text[i + 1])) return i;
            }
            return -1;
        }

        private static IReadOnlyList<PackageInfo> ParseZypperTable(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                string[] parts = line.Split('|');
                if (parts.Length < 5) continue;
                string name = parts[1].Trim();
                if (name.Length == 0 || name == "Name" || name == "--") continue;
                list.Add(new PackageInfo { Name = name, Version = parts[3].Trim(), IsInstalled = parts[0].Trim() == "i" });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParsePacmanSearch(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            PackageInfo current = null;
            foreach (string line in SplitLines(output))
            {
                if (char.IsWhiteSpace(line[0]))
                {
                    if (current != null && string.IsNullOrEmpty(current.Description)) current.Description = line.Trim();
                    continue;
                }
                int versionStart = line.IndexOf(' ');
                if (versionStart <= 0)
                {
                    current = null;
                    continue;
                }
                string name = StripPacmanRepo(line[..versionStart]);
                string version = line[versionStart..].Trim();
                if (version.Contains('['))
                {
                    bool installed = version.Contains("[installed", StringComparison.Ordinal);
                    version = version[..version.IndexOf('[')].Trim();
                    list.Add(current = new PackageInfo { Name = name, Version = version, IsInstalled = installed });
                    continue;
                }
                list.Add(current = new PackageInfo { Name = name, Version = version });
            }
            return list;
        }

        private static IReadOnlyList<PackageInfo> ParsePacmanInstalled(string output)
        {
            List<PackageInfo> list = new List<PackageInfo>();
            foreach (string line in SplitLines(output))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;
                list.Add(new PackageInfo { Name = parts[0], Version = parts[1], IsInstalled = true });
            }
            return list;
        }

        private static string StripDpkgArch(string name)
        {
            int index = name.IndexOf(':');
            return index > 0 ? name[..index] : name;
        }

        private static string StripArch(string name)
        {
            int index = name.LastIndexOf('.');
            if (index <= 0) return name;
            string suffix = name[(index + 1)..];
            return DnfArchSuffixes.Contains(suffix) ? name[..index] : name;
        }

        private static string StripPacmanRepo(string name)
        {
            int index = name.IndexOf('/');
            return index >= 0 ? name[(index + 1)..] : name;
        }

        private static IEnumerable<string> SplitLines(string output)
        {
            return string.IsNullOrEmpty(output) ? Enumerable.Empty<string>() : output.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Length > 0);
        }
    }
}
