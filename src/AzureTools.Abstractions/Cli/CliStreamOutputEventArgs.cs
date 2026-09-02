using System;

namespace AzureTools.Cli
{
    public class CliStreamOutputEventArgs : EventArgs
    {
        public string Line { get; }

        public CliStreamOutputEventArgs(string line)
        {
            Line = line;
        }
    }
}
