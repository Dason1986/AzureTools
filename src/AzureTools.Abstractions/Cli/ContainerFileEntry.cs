namespace AzureTools.Cli
{
    public class ContainerFileEntry
    {
        public string Permissions { get; set; } = string.Empty;
        public long HardLinkCount { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ModifyTime { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDirectory => Permissions.StartsWith("d");


    }
}
