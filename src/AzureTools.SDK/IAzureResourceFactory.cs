using Azure.Core;
using AzureTools.SDK.AppContainers;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.SDK.KeyVaults;
using AzureTools.SDK.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.SDK
{
    public interface IAzureResourceFactory
    {
        IKeyVaultManagement GetKeyVaultManagement(KeyVaultAzureEnv option, AzureTenant profile);
        IContainerRegistryManagement GetContainerRegistryManagement(ContainerRegistryAzureEnv option, AzureTenant profile);
        IAppContainerManagement GetAppContainerManagement(AppContainerAzureEnv option, AzureTenant profile);
        Task<KeyVaultAzureEnv[]> GetAllKeyVaultAsync(AzureTenant profile);
        Task<ContainerRegistryAzureEnv[]> GetAllContainerRegistriesAsync(AzureTenant profile);
        Task<AppContainerAzureEnv[]> GetAllAppContainersAsync(AzureTenant profile);

        TokenCredential CreateCredential(AzureTenant profile);
        Task<IDictionary<string, string>> TestCertificateAsync(AzureTenant profile);
        void ClearCredentialCache();
    }
}
