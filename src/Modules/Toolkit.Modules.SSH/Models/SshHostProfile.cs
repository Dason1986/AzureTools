using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Toolkit.Modules.SSH.Models
{
    public class SshHostProfile
    {
        [Key] public Guid Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; } = 22; public string UserName { get; set; }
        public string AuthMode { get; set; }
        public string Password { get; set; }
        public string PrivateKeyRawData { get; set; }
        public string Passphrase { get; set; }
        public string Description { get; set; }
        public string Os { get; set; }
        public string OsVersion { get; set; }
        public string OsId { get; set; }
        public Dictionary<string, string> Features { get; set; } = new();

        [JsonExtensionData]
        public Dictionary<string, System.Text.Json.JsonElement> LegacyData { get; set; }
    }
}
