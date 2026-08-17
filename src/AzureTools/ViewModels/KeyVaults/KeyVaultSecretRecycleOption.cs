using AzureTools.SDK.KeyVaults;

namespace AzureTools.ViewModels
{
    public class KeyVaultSecretRecycleOption
    {
        public IKeyVaultManagement KeyVault { get; set; }

        public string EnvironmentName { get; set; }
    }
}
