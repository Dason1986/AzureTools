using AzureTools.SDK.KeyVaults;

namespace AzureTools.Module.Azure.ViewModels.KeyVaults
{
    public class KeyVaultSecretRecycleOption
    {
        public IKeyVaultManagement KeyVault { get; set; }

        public string EnvironmentName { get; set; }
    }
}
