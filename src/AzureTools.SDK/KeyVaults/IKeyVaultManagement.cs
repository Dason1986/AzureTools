using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.SDK.KeyVaults
{
    public interface IKeyVaultManagement
    {
        string KeyVaultName { get; }

        Task Set(string key, string value);
        Task<string> Get(string key, string version = null);
        Task<List<SecretItem>> GetDeletedKeys();
        Task<List<SecretItem>> GetKeyVersion(SecretItem secret);
        Task<List<SecretItem>> GetKeys();
        Task SetExpiration(string key, DateTime value);
        Task PurgeAllDeletedSecretsAsync();
        Task PurgeDeletedSecretAsync(string secretName);
        Task<SecretItem> RecoverDeletedSecretAsync(string secretName);
    }
}
