using AzureTools.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Stores
{
    public class DefaultAppContainerStore : IAppContainerStore
    {
        #region Constructors

        public DefaultAppContainerStore(IStoreContext storeContext)
        {
            _storeContext = storeContext;
        }

        #endregion

        #region Fields & Properties

        private readonly IStoreContext _storeContext;

        #endregion

        public List<AppContainerEnv> GetOptions()
        {
            return _storeContext.GetSet<AppContainerEnv>().ToList();
        }

        public AppContainerEnv GetOption(string name)
        {
            return _storeContext.GetSet<AppContainerEnv>().FirstOrDefault(x => x.EnvironmentName == name);
        }

        public void AddOption(AppContainerEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _storeContext.Add(item);
        }

        public void UpdateOption(AppContainerEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            AppContainerEnv existingItem = _storeContext.GetSet<AppContainerEnv>()
                .FirstOrDefault(x => string.Equals(x.EnvironmentName, item.EnvironmentName, StringComparison.OrdinalIgnoreCase));
            if (existingItem != null)
            {
                existingItem.EnvironmentName = item.EnvironmentName;
                existingItem.TenantId = item.TenantId;
                existingItem.SubscriptionName = item.SubscriptionName;
                existingItem.SubscriptionId = item.SubscriptionId;
                existingItem.ResourceType = item.ResourceType;
                _storeContext.Update(existingItem);
            }
            else
            {
                _storeContext.Add(item);
            }
        }

        public void RemoveOption(AppContainerEnv item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _storeContext.Remove(item);
        }

        public void Save()
        {
            _storeContext.SaveChanges();
        }

        public void DiscardChanges()
        {
            _storeContext.DiscardChanges();
        }
    }
}
