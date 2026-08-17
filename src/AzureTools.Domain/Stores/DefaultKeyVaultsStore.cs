using AzureTools.Models;
using AzureTools.Models.KeyVaults;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Stores
{
    public class DefaultKeyVaultsStore : IKeyVaultStore
    {
        #region Constructors

        public DefaultKeyVaultsStore(IStoreContext storeContext)
        {
            this.storeContext = storeContext;

        }

        #endregion

        #region Fields & Properties

        private readonly IStoreContext storeContext;



        #endregion

        public List<KeyVaultEnv> GetOptions()
        {
            return storeContext.GetSet<KeyVaultEnv>().ToList();
        }

        public KeyVaultEnv GetOption(string name)
        {
            return storeContext.GetSet<KeyVaultEnv>().FirstOrDefault(x => x.Name == name);
        }

        public void AddOption(KeyVaultEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            storeContext.Add(item);
        }

        public void UpdateOption(KeyVaultEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            KeyVaultEnv existingItem = storeContext.GetSet<KeyVaultEnv>()
                .FirstOrDefault(x => string.Equals(x.Name, item.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItem != null)
            {
                existingItem.KeyVault = item.KeyVault;
                storeContext.Update(existingItem);
            }
            else
                storeContext.Add(item);
        }

        public void RemoveOption(KeyVaultEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            storeContext.Remove(item);
        }

        public virtual SecretKeyOptionItem[] GetRequiredKeys(Guid projectId)
        {
            return storeContext.GetSet<SecretKeyOptionItem>().Where(x => x.ProjectId == projectId).ToArray();
        }


        public void Import(IEnumerable<SecretKeyOptionItem> items)
        {
            List<SecretKeyOptionItem> existing = storeContext.GetSet<SecretKeyOptionItem>().ToList();
            Dictionary<string, SecretKeyOptionItem> lookup = existing.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

            foreach (SecretKeyOptionItem item in items)
            {
                if (lookup.TryGetValue(item.Key, out SecretKeyOptionItem existingItem))
                {
                    existingItem.Required = item.Required;
                    existingItem.Type = item.Type;
                    existingItem.Description = item.Description;
                    existingItem.Option = item.Option;
                    existingItem.Tags = item.Tags;
                    storeContext.Update(existingItem);
                }
                else
                    storeContext.Add(item);
            }

            storeContext.SaveChanges();
        }
        public void AddKey(SecretKeyOptionItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            storeContext.Add(item);
        }

        public void UpdateKey(SecretKeyOptionItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            SecretKeyOptionItem existingItem = storeContext.GetSet<SecretKeyOptionItem>()
                .FirstOrDefault(x => string.Equals(x.Key, item.Key, StringComparison.OrdinalIgnoreCase) && x.ProjectId == item.ProjectId);
            if (existingItem != null)
            {
                existingItem.Required = item.Required;
                existingItem.Type = item.Type;
                existingItem.Description = item.Description;
                existingItem.Option = item.Option;
                existingItem.Tags = item.Tags;
                storeContext.Update(existingItem);
            }
            else
                storeContext.Add(item);
        }

        public void RemoveKey(SecretKeyOptionItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            storeContext.Remove(item);
        }

        public void Save()
        {
            storeContext.SaveChanges();
        }

        public void DiscardChanges()
        {
            storeContext.DiscardChanges();
        }






    }
}
