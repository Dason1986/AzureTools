using AzureTools.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Stores
{
    public class DefaultContainerRegistryStore : IContainerRegistryStore
    {
        #region Constructors

        public DefaultContainerRegistryStore(IStoreContext storeContext)
        {
            this.storeContext = storeContext;
        }

        #endregion

        #region Fields & Properties

        private readonly IStoreContext storeContext;

        #endregion

        public List<ContainerRegistryEnv> GetOptions()
        {
            return storeContext.GetSet<ContainerRegistryEnv>().ToList();
        }

        public ContainerRegistryEnv GetOption(string name)
        {
            return storeContext.GetSet<ContainerRegistryEnv>().FirstOrDefault(x => x.Name == name);
        }

        public void AddOption(ContainerRegistryEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            storeContext.Add(item);
        }

        public void UpdateOption(ContainerRegistryEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            ContainerRegistryEnv existingItem = storeContext.GetSet<ContainerRegistryEnv>()
                .FirstOrDefault(x => string.Equals(x.Name, item.Name, StringComparison.OrdinalIgnoreCase));
            if (existingItem != null)
            {
                existingItem.Registry = item.Registry;
                existingItem.RegistryUrl = item.RegistryUrl;
                existingItem.CreatedOn = item.CreatedOn;
                existingItem.IsAdminUserEnabled = item.IsAdminUserEnabled;
                existingItem.IsAnonymousPullEnabled = item.IsAnonymousPullEnabled;
                existingItem.TenantId = item.TenantId;
                existingItem.SubscriptionName = item.SubscriptionName;
                existingItem.SubscriptionId = item.SubscriptionId;
                storeContext.Update(existingItem);
            }
            else
            {
                storeContext.Add(item);
            }
        }

        public void RemoveOption(ContainerRegistryEnv item)
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
