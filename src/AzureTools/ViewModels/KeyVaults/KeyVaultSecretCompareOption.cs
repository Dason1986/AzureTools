
using AzureTools.Services.KeyVaults;
using System.Collections.Generic;

namespace AzureTools.ViewModels
{
    public class KeyVaultSecretCompareOption
    {
        public List<string> EnvNames { get; set; }

        public List<SecretCompareReport> Results { get; set; }
    }
}
