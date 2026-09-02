using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.ContainerRegistry;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using AzureTools.Extensions;
using AzureTools.Properties;
using AzureTools.SDK.AppContainers;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.SDK.KeyVaults;
using AzureTools.SDK.Models;
using AzureTools.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.SDK
{
    public class AzureResourceFactory : IAzureResourceFactory
    {
        public AzureResourceFactory(IDialogService dialogService)
        {
            _dialogService = dialogService;
            credentialCaches = new Dictionary<string, TokenCredential>();
        }

        private readonly IDialogService _dialogService;
        private readonly IDictionary<string, TokenCredential> credentialCaches;

        public IKeyVaultManagement GetKeyVaultManagement(KeyVaultAzureEnv option, AzureTenant profile)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNull(profile);
            TokenCredential credential = this.GetCredential(profile);
            return new KeyVaultManagement(option.KeyVault, option.KeyVaultUrl, credential);
        }

        public IContainerRegistryManagement GetContainerRegistryManagement(ContainerRegistryAzureEnv option, AzureTenant profile)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNull(profile);
            TokenCredential credential = this.GetCredential(profile);

            return new ContainerRegistryManagement(option, credential);
        }

        public IAppContainerManagement GetAppContainerManagement(AppContainerAzureEnv option, AzureTenant profile)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNull(profile);
            TokenCredential credential = this.GetCredential(profile);
            return new AppContainerManagement(option, credential);
        }

        public async Task<KeyVaultAzureEnv[]> GetAllKeyVaultAsync(AzureTenant profile)
        {
            TokenCredential credential = this.GetCredential(profile);
            ArmClient _armClient = new ArmClient(credential);
            SubscriptionCollection subscriptions = _armClient.GetSubscriptions();
            List<KeyVaultAzureEnv> options = new List<KeyVaultAzureEnv>();
            foreach (SubscriptionResource subscription in subscriptions)
            {
                await foreach (KeyVaultResource vault in subscription.GetKeyVaultsAsync())
                {
                    KeyVaultData data = vault.Data;

                    options.Add(new KeyVaultAzureEnv
                    {
                        Name = data.Name,
                        KeyVault = data.Name,
                        CreatedOn = vault.Data.SystemData.CreatedOn.ToLocalDateTime(),
                        ResourceType = data.ResourceType.ToString(),
                        ResourceGroupName = data.Id.ResourceGroupName,
                        KeyVaultUrl = data.Properties.VaultUri.ToString(),
                        SubscriptionName = subscription.Data.DisplayName,
                        SubscriptionId = subscription.Data.SubscriptionId,
                        EnableSoftDelete = data.Properties.EnableSoftDelete
                    });
                }
            }
            return options.ToArray();
        }

        public async Task<AppContainerAzureEnv[]> GetAllAppContainersAsync(AzureTenant profile)
        {
            TokenCredential credential = this.GetCredential(profile);
            ArmClient armClient = new ArmClient(credential);
            SubscriptionCollection subscriptions = armClient.GetSubscriptions();
            List<AppContainerAzureEnv> options = new List<AppContainerAzureEnv>();
            foreach (SubscriptionResource subscription in subscriptions)
            {
                await foreach (ContainerAppManagedEnvironmentResource vault in subscription.GetContainerAppManagedEnvironmentsAsync())
                {
                    ContainerAppManagedEnvironmentData data = vault.Data;
                    options.Add(new AppContainerAzureEnv
                    {
                        Name = data.Name,
                        EnvironmentName = data.Name,
                        CreatedOn = vault.Data.SystemData.CreatedOn.ToLocalDateTime(),
                        ResourceType = data.ResourceType.ToString(),
                        ResourceGroupName = data.Id.ResourceGroupName,
                        SubscriptionName = subscription.Data.DisplayName,
                        SubscriptionId = subscription.Data.SubscriptionId,
                    });
                }
            }
            return options.ToArray();
        }

        public async Task<ContainerRegistryAzureEnv[]> GetAllContainerRegistriesAsync(AzureTenant profile)
        {
            TokenCredential credential = this.GetCredential(profile);
            ArmClient _armClient = new ArmClient(credential);
            SubscriptionCollection subscriptions = _armClient.GetSubscriptions();
            List<ContainerRegistryAzureEnv> options = new List<ContainerRegistryAzureEnv>();
            foreach (SubscriptionResource subscription in subscriptions)
            {
                await foreach (ContainerRegistryResource vault in subscription.GetContainerRegistriesAsync())
                {
                    ContainerRegistryData data = vault.Data;

                    options.Add(new ContainerRegistryAzureEnv
                    {
                        Name = data.Name,
                        ResourceType = data.ResourceType.ToString(),
                        ResourceGroupName = data.Id.ResourceGroupName,
                        Registry = data.LoginServer,
                        RegistryUrl = $"https://{data.LoginServer}",
                        CreatedOn = vault.Data.SystemData.CreatedOn.ToLocalDateTime(),
                        IsAdminUserEnabled = data.IsAdminUserEnabled,
                        IsAnonymousPullEnabled = data.IsAnonymousPullEnabled,
                        SubscriptionName = subscription.Data.DisplayName,
                        SubscriptionId = subscription.Data.SubscriptionId,
                    });
                }
            }
            return options.ToArray();
        }

        private async Task SafeCopyToClipboardAsync(DeviceCodeInfo info, CancellationToken cancellationToken)
        {
            bool result = await _dialogService.SetClipboardAsync(info.UserCode);
            await _dialogService.OpenWithExplorerAsync(info.VerificationUri.ToString());
            await _dialogService.ShowInfoAsync(string.Format(SDKResources.BrowserLoginPrompt, info.UserCode, result ? SDKResources.AlreadyCopiedToClipboard : string.Empty));
        }

        public TokenCredential CreateCredential(AzureTenant profile)
        {
            TokenCredential tokenCredential = null;
            switch (profile.Credential)
            {
                case AzureCredential.Browser:
                    tokenCredential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions { TenantId = profile.TenantId });
                    break;
                case AzureCredential.AzureCli:
                    tokenCredential = new AzureCliCredential(new AzureCliCredentialOptions { TenantId = profile.TenantId });
                    break;
                case AzureCredential.DeviceCode:
                    tokenCredential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
                    {
                        TenantId = profile.TenantId,
                        DeviceCodeCallback = SafeCopyToClipboardAsync
                    });
                    break;
                case AzureCredential.Certificate:
                case AzureCredential.CertificatePassword:
                    {
                        tokenCredential = CreateX509Credential(profile);
                        break;
                    }
                default:
                    throw new NotSupportedException(SDKResources.NotSupportedMode);
            }
            return tokenCredential;
        }

        protected virtual TokenCredential CreateX509Credential(AzureTenant profile)
        {
            TokenCredential tokenCredential;
            byte[] rawData = null;

            string certificateRawData = profile.CertificateRawData;

            if (!string.IsNullOrEmpty(certificateRawData)) rawData = Convert.FromBase64String(certificateRawData);
            if (certificateRawData == null) throw new Exception(SDKResources.CertificateFileNotFound);
            string password = profile.Password;
            if (profile.Credential == AzureCredential.CertificatePassword && string.IsNullOrWhiteSpace(password)) throw new Exception(SDKResources.CertPasswordEmpty);
            try
            {
                X509Certificate2 certificate = string.IsNullOrWhiteSpace(password) ? X509CertificateLoader.LoadPkcs12(rawData, "") : X509CertificateLoader.LoadPkcs12(rawData, password);
                tokenCredential = new ClientCertificateCredential(profile.TenantId, profile.ClientId, certificate);
            }
            catch (Exception ex)
            {
                throw new Exception(SDKResources.CertificateLoadFailed, ex);
            }

            return tokenCredential;
        }

        private TokenCredential GetCredential(AzureTenant profile)
        {
            if (credentialCaches.TryGetValue(profile.TenantId, out TokenCredential val)) return val;
            TokenCredential credential = CreateCredential(profile);
            credentialCaches.Add(profile.TenantId, credential);
            return credential;
        }

        public async Task<IDictionary<string, string>> TestCertificateAsync(AzureTenant profile)
        {
            TokenCredential tokenCredential = CreateCredential(profile);

            AccessToken token = await tokenCredential.GetTokenAsync(new Azure.Core.TokenRequestContext(new string[] { "https://graph.microsoft.com/.default" }), CancellationToken.None);
            if (string.IsNullOrEmpty(token.Token)) return new Dictionary<string, string>();
            JwtSecurityToken jwtToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token.Token);
            return jwtToken.Claims.Select(x => new { Type = x.Type, Value = x.Value }).DistinctBy(x => x.Type).ToDictionary(x => x.Type, x => x.Value, StringComparer.OrdinalIgnoreCase
                );

        }

        public void ClearCredentialCache()
        {
            credentialCaches.Clear();
        }
    }
}
