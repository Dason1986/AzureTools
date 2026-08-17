using AzureTools.Models;
using AzureTools.Models.KeyVaults;
using System;
using System.Collections.Generic;

namespace AzureTools.Stores
{
    public interface IKeyVaultStore
    {

        List<KeyVaultEnv> GetOptions();
        KeyVaultEnv GetOption(string name);
        void AddOption(KeyVaultEnv item);
        void UpdateOption(KeyVaultEnv item);
        void RemoveOption(KeyVaultEnv item);
        SecretKeyOptionItem[] GetRequiredKeys(Guid projectId);
        void Import(IEnumerable<SecretKeyOptionItem> items);
        void AddKey(SecretKeyOptionItem item);
        void UpdateKey(SecretKeyOptionItem item);
        void RemoveKey(SecretKeyOptionItem item);
        void Save();
        void DiscardChanges();
    }

}
