using AzureTools.SDK.KeyVaults;
using System.Collections.Generic;

namespace AzureTools.ViewModels
{
    public class KeyVaultSecretHistoriesOption
    {
        public List<SecretItem> Secrets { get; set; }

        public string EnvironmentName { get; set; }
    }
}
