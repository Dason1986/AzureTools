using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using AzureTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.SDK.KeyVaults
{
    public class KeyVaultManagement : IKeyVaultManagement
    {
        public KeyVaultManagement(string keyVaultName, string keyVaultUrk, TokenCredential credential, SecretClient client = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(keyVaultName);
            ArgumentException.ThrowIfNullOrEmpty(keyVaultUrk);
            if (!Uri.TryCreate(keyVaultUrk, UriKind.RelativeOrAbsolute, out Uri _)) throw new ArgumentException(keyVaultUrk);
            KeyVaultName = keyVaultName;
            _kvUri = keyVaultUrk;
            _client = client ?? new SecretClient(new Uri(_kvUri), credential);
        }

        public KeyVaultManagement(string keyVaultName, TokenCredential credential, SecretClient client = null) : this(keyVaultName, $"https://{keyVaultName}.vault.azure.net", credential, client)
        {
        }

        private readonly SecretClient _client;
        private readonly string _kvUri;

        public string KeyVaultName { get; private set; }

        public async Task Set(string key, string value)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            ArgumentException.ThrowIfNullOrEmpty(value);
            await _client.SetSecretAsync(key, value);
        }

        public async Task<string> Get(string key, string version = null)
        {
            Response<KeyVaultSecret> secret = await _client.GetSecretAsync(key, version);
            if (secret == null || secret.Value == null) return null;
            return secret.Value.Value;
        }

        public async Task<List<SecretItem>> GetDeletedKeys()
        {
            AsyncPageable<DeletedSecret> secrets = _client.GetDeletedSecretsAsync();
            List<SecretItem> list = new List<SecretItem>();
            await foreach (Page<DeletedSecret> property in secrets.AsPages())
            {
                list.AddRange(property.Values.Select(CastDelete));
            }
            return list;
        }

        public async Task<SecretItem> RecoverDeletedSecretAsync(string secretName)
        {
            RecoverDeletedSecretOperation op = await _client.StartRecoverDeletedSecretAsync(secretName);
            Response<SecretProperties> secret = await op.WaitForCompletionAsync();
            return Cast(secret);
        }

        public async Task PurgeAllDeletedSecretsAsync()
        {
            List<SecretItem> deletedList = await GetDeletedKeys();
            foreach (SecretItem secret in deletedList)
            {
                await PurgeDeletedSecretAsync(secret.Name);
            }
        }

        public async Task PurgeDeletedSecretAsync(string secretName)
        {
            await _client.PurgeDeletedSecretAsync(secretName);
        }

        public async Task<List<SecretItem>> GetKeyVersion(SecretItem secret)
        {
            string key = secret.Name;
            AsyncPageable<SecretProperties> secrets = _client.GetPropertiesOfSecretVersionsAsync(key);
            List<SecretItem> list = new List<SecretItem>();
            await foreach (Page<SecretProperties> property in secrets.AsPages())
            {
                list.AddRange(property.Values.Select(Cast));
            }
            if (list.Count <= 1) return list;
            foreach (SecretItem item in list)
            {
                item.Value = await Get(item.Name, item.Version);
            }
            return list;
        }

        protected virtual SecretItem CastDelete(DeletedSecret deletedSecret)
        {
            SecretItem secret = Cast(deletedSecret.Properties);
            secret.Deleted = deletedSecret.DeletedOn.HasValue ? deletedSecret.DeletedOn.Value.AddHours(8) : new DateTimeOffset?();
            return secret;
        }

        protected virtual SecretItem Cast(SecretProperties properties)
        {
            return new SecretItem
            {
                Name = properties.Name,
                Version = properties.Version,
                Enabled = properties.Enabled.GetValueOrDefault(),
                Created = properties.CreatedOn.ToLocalDateTime(),
                Updated = properties.UpdatedOn.ToLocalDateTime(),
                Expires = properties.ExpiresOn.ToLocalDateTime(),
            };
        }

        public async Task<List<SecretItem>> GetKeys()
        {
            AsyncPageable<SecretProperties> secrets = _client.GetPropertiesOfSecretsAsync();
            List<SecretItem> list = await secrets.Select(Cast).ToListAsync();
            return list;
        }

        public async Task SetExpiration(string key, DateTime value)
        {
            Response<KeyVaultSecret> secret = await _client.GetSecretAsync(key);
            if (secret == null || secret.Value == null) return;
            secret.Value.Properties.ExpiresOn = value.Date;
            await _client.SetSecretAsync(secret);
        }
    }
}
