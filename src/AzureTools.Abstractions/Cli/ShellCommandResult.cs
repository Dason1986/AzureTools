using System.Collections.Generic;

namespace AzureTools.Cli
{
    public class ShellCommandResult
    {
        public List<string> OutputLines { get; set; } = new();
        public string RawOutput { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
