using AzureTools.SDK.KeyVaults;
using System.Collections.Generic;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public class KeyVaultSecretHistoriesOption
    {
        public List<SecretItem> Secrets { get; set; }

        public string EnvironmentName { get; set; }
    }
}
