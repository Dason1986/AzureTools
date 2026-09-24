using AzureTools.SDK.KeyVaults;

namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public class KeyVaultSecretRecycleOption
    {
        public IKeyVaultManagement KeyVault { get; set; }

        public string EnvironmentName { get; set; }
    }
}
