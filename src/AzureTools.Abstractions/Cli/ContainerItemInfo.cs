using System;

namespace AzureTools.Cli
{
    public class ContainerItemInfo
    {
        public ContainerItemInfo(string name, string fullPath, long length, DateTimeOffset lastModified, bool isDirectory)
        {
            Name = name ?? string.Empty;
            FullPath = fullPath ?? string.Empty;
            Length = length;
            LastModified = lastModified;
            IsDirectory = isDirectory;
        }

        public string Name { get; }

        public string FullPath { get; }

        public long Length { get; }

        public DateTimeOffset LastModified { get; }

        public bool IsDirectory { get; }

        public bool IsFile => !IsDirectory;

        public bool Exists => !string.IsNullOrEmpty(Name);
    }
}
