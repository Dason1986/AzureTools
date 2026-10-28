using System;
using System.Collections.Generic;
using System.Linq;

namespace Toolkit.Modules.SSH.Models
{
    public class FirewallBackendCatalog
    {
        public const string Ufw = "ufw";
        public const string Firewalld = "firewalld";
        public const string Iptables = "iptables";

        public string Kind { get; }

        public string Binary { get; }

        public string StatusCommand { get; }

        public string ListCommand { get; }

        public string AddCommandFormat { get; }

        public string RemoveByNumberCommandFormat { get; }

        public string RemoveByPortCommandFormat { get; }

        public string EnableCommand { get; }

        public string DisableCommand { get; }

        public bool SupportsToggle { get; }

        public Func<string, bool> StatusParser { get; }

        public Func<string, IReadOnlyList<FirewallRule>> ListParser { get; }

        private FirewallBackendCatalog(string kind, string binary, string statusCommand, string listCommand, string addCommandFormat, string removeByNumberCommandFormat, string removeByPortCommandFormat, string enableCommand, string disableCommand, bool supportsToggle, Func<string, bool> statusParser, Func<string, IReadOnlyList<FirewallRule>> listParser)
        {
            Kind = kind;
            Binary = binary;
            StatusCommand = statusCommand;
            ListCommand = listCommand;
            AddCommandFormat = addCommandFormat;
            RemoveByNumberCommandFormat = removeByNumberCommandFormat;
            RemoveByPortCommandFormat = removeByPortCommandFormat;
            EnableCommand = enableCommand;
            DisableCommand = disableCommand;
            SupportsToggle = supportsToggle;
            StatusParser = statusParser;
            ListParser = listParser;
        }

        public static FirewallBackendCatalog Find(string kind)
            => All.FirstOrDefault(c => c.Kind == kind);

        public static readonly FirewallBackendCatalog[] All =
        {
            new FirewallBackendCatalog(
                Ufw, "ufw",
                "ufw status",
                "ufw status numbered",
                "ufw {2} {0}/{1}",
                "ufw --force delete {0}",
                null,
                "ufw --force enable",
                "ufw disable",
                true,
                ParseUfwStatus, ParseUfwRules),
            new FirewallBackendCatalog(
                Firewalld, "firewall-cmd",
                "firewall-cmd --state",
                "firewall-cmd --list-ports; echo ===; firewall-cmd --list-services",
                "firewall-cmd --permanent --add-port={0}/{1} && firewall-cmd --reload",
                null,
                "firewall-cmd --permanent --remove-port={0}/{1} && firewall-cmd --reload",
                "systemctl enable --now firewalld",
                "systemctl disable --now firewalld",
                true,
                ParseFirewalldStatus, ParseFirewalldRules),
            new FirewallBackendCatalog(
                Iptables, "iptables",
                "echo active",
                "iptables -L INPUT -n --line-numbers",
                "iptables -A INPUT -p {1} --dport {0} -j {3}",
                "iptables -D INPUT {0}",
                null,
                null,
                null,
                false,
                ParseIptablesStatus, ParseIptablesRules)
        };

        private static bool ParseUfwStatus(string output)
            => SplitLines(output).Any(l => l == "Status: active");

        private static IReadOnlyList<FirewallRule> ParseUfwRules(string output)
        {
            List<FirewallRule> list = new List<FirewallRule>();
            foreach (string line in SplitLines(output))
            {
                string text = line.Replace("(v6)", "").Trim();
                if (!text.StartsWith("[", StringComparison.Ordinal)) continue;
                int end = text.IndexOf(']');
                if (end < 0) continue;
                string number = text[1..end].Trim();
                string[] tokens = text[(end + 1)..].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) continue;
                string port = tokens[0];
                string protocol = null;
                int slash = port.IndexOf('/');
                if (slash > 0)
                {
                    protocol = port[(slash + 1)..];
                    port = port[..slash];
                }
                string direction = tokens.Length > 2 && tokens[2] == "OUT" ? "出站" : "入站";
                int sourceStart = tokens.Length > 2 && (tokens[2] == "IN" || tokens[2] == "OUT") ? 3 : 2;
                list.Add(new FirewallRule
                {
                    Number = number,
                    Port = port,
                    Protocol = protocol,
                    Action = MapAction(tokens[1]),
                    Direction = direction,
                    Source = tokens.Length > sourceStart ? string.Join(" ", tokens[sourceStart..]) : null,
                    Raw = line
                });
            }
            return list;
        }

        private static bool ParseFirewalldStatus(string output)
            => output.Trim() == "running";

        private static IReadOnlyList<FirewallRule> ParseFirewalldRules(string output)
        {
            List<FirewallRule> list = new List<FirewallRule>();
            string[] sections = output.Split(new[] { "===" }, StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length > 0)
            {
                foreach (string token in SplitTokens(sections[0]))
                {
                    int slash = token.IndexOf('/');
                    if (slash <= 0) continue;
                    list.Add(new FirewallRule
                    {
                        Port = token[..slash],
                        Protocol = token[(slash + 1)..],
                        Action = "允许",
                        Direction = "入站",
                        Raw = token
                    });
                }
            }
            if (sections.Length > 1)
            {
                foreach (string token in SplitTokens(sections[1]))
                {
                    list.Add(new FirewallRule
                    {
                        Port = token,
                        Protocol = "service",
                        Action = "允许",
                        Direction = "入站",
                        Raw = token
                    });
                }
            }
            return list;
        }

        private static bool ParseIptablesStatus(string output)
            => output.Trim() == "active";

        private static IReadOnlyList<FirewallRule> ParseIptablesRules(string output)
        {
            List<FirewallRule> list = new List<FirewallRule>();
            foreach (string line in SplitLines(output))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6 || !int.TryParse(parts[0], out _)) continue;
                string extras = string.Join(" ", parts[6..]);
                list.Add(new FirewallRule
                {
                    Number = parts[0],
                    Port = ExtractPort(extras),
                    Protocol = parts[2] == "all" ? null : parts[2],
                    Action = MapAction(parts[1]),
                    Direction = "入站",
                    Source = parts[4],
                    Raw = line
                });
            }
            return list;
        }

        private static string ExtractPort(string extras)
        {
            string[] tokens = extras.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].StartsWith("dpt:", StringComparison.Ordinal)) return tokens[i][4..];
                if (tokens[i].StartsWith("dpts:", StringComparison.Ordinal)) return tokens[i][5..];
                if (tokens[i] == "dports" && i + 1 < tokens.Length) return tokens[i + 1];
            }
            return null;
        }

        private static string MapAction(string target)
        {
            return target.ToUpperInvariant() switch
            {
                "ALLOW" or "ACCEPT" => "允许",
                "DENY" or "REJECT" or "DROP" => "拒绝",
                "LIMIT" => "限制",
                _ => target
            };
        }

        private static IEnumerable<string> SplitLines(string output)
        {
            return string.IsNullOrEmpty(output)
                ? Enumerable.Empty<string>()
                : output.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l.Trim().Length > 0);
        }

        private static IEnumerable<string> SplitTokens(string output)
        {
            return string.IsNullOrWhiteSpace(output)
                ? Enumerable.Empty<string>()
                : output.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
