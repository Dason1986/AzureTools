using System;
using System.Collections.Generic;

namespace AzureTools.SDK.ContainerRegistries
{
    public class ManifestDetail
    {
        public string RepositoryName { get; set; }
        public string Digest { get; set; }
        public long? SizeInBytes { get; set; }
        public DateTimeOffset? CreatedOn { get; set; }
        public DateTimeOffset? LastUpdatedOn { get; set; }
        public string Architecture { get; set; }
        public string OperatingSystem { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public string Comment { get; set; }
        public string Parent { get; set; }
        public string[] Entrypoint { get; set; } = Array.Empty<string>();
        public string Author { get; set; }
        public IDictionary<string, string> ExposedPorts { get; set; } = new Dictionary<string, string>();
        public IList<string> Volumes { get; set; } = new List<string>();
        public IDictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
        public IList<string> Layers { get; set; } = new List<string>();
    }
}
