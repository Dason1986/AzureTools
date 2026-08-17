using AzureTools.Models;
using AzureTools.SDK;
using AzureTools.Stores;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AzureTools.Tests.Stubs
{

    class SubStoreContext : IStoreContext
    {
        private readonly IDictionary<Type, object> _cache = new ConcurrentDictionary<Type, object>();

        public bool Encrypted { get; private set; }



        public void EnableEncryption()
        {
            Encrypted = true;
        }

        public SubStoreContext()
        {
            AppProfile profile = new AppProfile()
            {
                Name = "Azure Tools",
                TenantMode = TenantMode.Single,

            };
            TenantProfile[] Tenants = [new TenantProfile { Credential = AzureCredential.Browser.ToString() }];
            KeyVaultEnv[] keyVaultEnvs = new KeyVaultEnv[]
            {
                new KeyVaultEnv { KeyVault = "kv-intrextrcms-uat-01", Name = "UAT" },
                new KeyVaultEnv { KeyVault = "kv-intrextrcms-prod-01", Name = "Prod" }
            };
            _cache[typeof(AppProfile)] = new List<AppProfile> { profile };
            _cache[typeof(KeyVaultEnv)] = keyVaultEnvs.ToList();
            _cache[typeof(TenantProfile)] = Tenants.ToList();
        }

        public IStoreSet<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return GetList<TEntity>();
        }

        public TEntity Find<TEntity>() where TEntity : class
        {
            return GetSet<TEntity>().FirstOrDefault();
        }

        public void Add<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            GetList<TEntity>().Add(item);
        }

        public void AddRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            GetList<TEntity>().AddRange(items);

        }

        public void Update<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            GetList<TEntity>().Update(item);

        }

        public void Remove<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            IStoreSet<TEntity> list = GetList<TEntity>();
            list.Remove(item);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            IStoreSet<TEntity> list = GetList<TEntity>();
            foreach (TEntity item in items)
                list.Remove(item);
        }

        public void SaveChanges()
        {
        }

        public void DiscardChanges()
        {
            _cache.Clear();
        }

        private IStoreSet<TEntity> GetList<TEntity>() where TEntity : class
        {
            if (_cache.TryGetValue(typeof(TEntity), out object value) && value is MemoryStoreSet<TEntity> list)
                return list;
            list = new MemoryStoreSet<TEntity>(new List<TEntity>());
            _cache[typeof(TEntity)] = list;
            return list;
        }
    }
}
