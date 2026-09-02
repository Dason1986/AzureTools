using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    class ContainerShell : IContainerShell
    {
        private readonly ContainerInteractiveSession _inner;

        public ContainerShell(ContainerInteractiveSession inner)
        {
            _inner = inner;
        }

        public async Task<List<ContainerFileEntry>> LsAsync(string path = ".", CancellationToken ct = default)
        {
            ShellCommandResult res = await _inner.SendCommandAsync($"ls -l {ShellEscape(path)}", ct);
            List<ContainerFileEntry> list = new List<ContainerFileEntry>();

            foreach (string line in res.OutputLines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("total")) continue;
                string[] parts = Regex.Split(line, @"\s+");
                if (parts.Length < 9) continue;

                _ = long.TryParse(parts[1], out long linkCnt);
                _ = long.TryParse(parts[4], out long size);

                list.Add(new ContainerFileEntry
                {
                    Permissions = parts[0],
                    HardLinkCount = linkCnt,
                    Owner = parts[2],
                    Group = parts[3],
                    Size = size,
                    ModifyTime = $"{parts[5]} {parts[6]} {parts[7]}",
                    Name = string.Join(" ", parts.Skip(8))
                });
            }
            return list;
        }

        public async Task<string> PwdAsync(CancellationToken ct = default)
        {
            ShellCommandResult r = await _inner.SendCommandAsync("pwd", ct);
            return r.OutputLines.FirstOrDefault()?.Trim() ?? string.Empty;
        }

        public async Task<string> CatAsync(string filePath, CancellationToken ct = default)
        {
            ShellCommandResult r = await _inner.SendCommandAsync($"cat {ShellEscape(filePath)}", ct);
            return r.RawOutput;
        }

        public async Task<ShellCommandResult> RawAsync(string cmd, CancellationToken ct = default)
        {
            return await _inner.SendCommandAsync(cmd, ct);
        }

        public void Disconnect() => _inner.Close();

        private static string ShellEscape(string arg)
        {
            return $"'{arg.Replace("'", @"'\''")}'";
        }
    }
}
