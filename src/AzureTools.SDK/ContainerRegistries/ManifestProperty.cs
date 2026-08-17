using System;

namespace AzureTools.SDK.ContainerRegistries
{
    public partial class ManifestProperty
    {
        public string RepositoryName { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public string Architecture { get; set; }
        public DateTimeOffset LastUpdatedOn { get; set; }
        public string Digest { get; set; }

        public bool CanDelete { get; set; }
        public long? SizeInBytes { get; set; }
        public string[] Tags { get; set; }
    }
}
