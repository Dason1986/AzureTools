using AzureTools.Models;
using AzureTools.SDK;
using AzureTools.SDK.Models;
using System;

namespace AzureTools.Extensions
{
    public static class SdkModelMappingExtensions
    {
        public static AzureTenant ToSdk(this TenantProfile source)
        {
            if (source == null) return null;
            return new AzureTenant
            {
                TenantId = source.TenantId,
                Name = source.Name,
                Code = source.Code,
                Credential = Enum.TryParse<AzureCredential>(source.Credential, true, out AzureCredential credential) ? credential : AzureCredential.Browser,
                ClientId = source.ClientId,
                CertificateRawData = source.CertificateRawData,
                Password = source.Password
            };
        }

        public static TenantProfile ToDomain(this AzureTenant source)
        {
            if (source == null) return null;
            return new TenantProfile
            {
                TenantId = source.TenantId,
                Name = source.Name,
                Code = source.Code,
                Credential = source.Credential.ToString(),
                ClientId = source.ClientId,
                CertificateRawData = source.CertificateRawData,
                Password = source.Password
            };
        }

        public static KeyVaultAzureEnv ToSdk(this KeyVaultEnv source)
        {
            if (source == null) return null;
            return new KeyVaultAzureEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                KeyVault = source.KeyVault,
                KeyVaultUrl = source.KeyVaultUrl,
                EnableSoftDelete = source.EnableSoftDelete
            };
        }

        public static KeyVaultEnv ToDomain(this KeyVaultAzureEnv source)
        {
            if (source == null) return null;
            return new KeyVaultEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                KeyVault = source.KeyVault,
                KeyVaultUrl = source.KeyVaultUrl,
                EnableSoftDelete = source.EnableSoftDelete
            };
        }

        public static AppContainerAzureEnv ToSdk(this AppContainerEnv source)
        {
            if (source == null) return null;
            return new AppContainerAzureEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                EnvironmentName = source.EnvironmentName
            };
        }

        public static AppContainerEnv ToDomain(this AppContainerAzureEnv source)
        {
            if (source == null) return null;
            return new AppContainerEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                EnvironmentName = source.EnvironmentName
            };
        }

        public static ContainerRegistryAzureEnv ToSdk(this ContainerRegistryEnv source)
        {
            if (source == null) return null;
            return new ContainerRegistryAzureEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                Registry = source.Registry,
                RegistryUrl = source.RegistryUrl,
                IsAdminUserEnabled = source.IsAdminUserEnabled,
                IsAnonymousPullEnabled = source.IsAnonymousPullEnabled
            };
        }

        public static ContainerRegistryEnv ToDomain(this ContainerRegistryAzureEnv source)
        {
            if (source == null) return null;
            return new ContainerRegistryEnv
            {
                Id = source.Id,
                Name = source.Name,
                CreatedOn = source.CreatedOn,
                TenantId = source.TenantId,
                SubscriptionName = source.SubscriptionName,
                SubscriptionId = source.SubscriptionId,
                ResourceType = source.ResourceType,
                ResourceGroupName = source.ResourceGroupName,
                Registry = source.Registry,
                RegistryUrl = source.RegistryUrl,
                IsAdminUserEnabled = source.IsAdminUserEnabled,
                IsAnonymousPullEnabled = source.IsAnonymousPullEnabled
            };
        }


    }
}
