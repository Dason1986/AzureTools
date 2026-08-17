
using System;

namespace AzureTools.SDK.KeyVaults
{
    public class SecretItem
    {
        public string Name { get; set; }


        public string Value { get; set; }
        public string Version { get; set; }
        public bool Enabled { get; set; }
        public bool Required { get; set; }


        public DateTimeOffset? Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
    }
}
