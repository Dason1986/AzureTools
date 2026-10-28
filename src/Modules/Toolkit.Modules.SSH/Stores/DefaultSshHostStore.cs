using AzureTools.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Stores
{
    class DefaultSshHostStore : ISshHostStore
    {
        private readonly IStoreContext storeContext; public DefaultSshHostStore(IStoreContext storeContext)
        {
            this.storeContext = storeContext;
        }
        public List<SshHostProfile> GetHosts()
        {
            List<SshHostProfile> hosts = storeContext.GetSet<SshHostProfile>().ToList();
            foreach (SshHostProfile host in hosts)
            {
                host.MigrateLegacyData();
            }
            return hosts;
        }
        public SshHostProfile GetHost(Guid id)
        {
            SshHostProfile host = storeContext.GetSet<SshHostProfile>().FirstOrDefault(x => x.Id == id);
            host?.MigrateLegacyData();
            return host;
        }
        public void AddOrUpdate(SshHostProfile host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host)); if (host.Id == Guid.Empty) host.Id = Guid.NewGuid();
            SshHostProfile existing = storeContext.GetSet<SshHostProfile>().FirstOrDefault(x => x.Id == host.Id); if (existing != null)
            {
                existing.Name = host.Name;
                existing.Host = host.Host;
                existing.Port = host.Port;
                existing.UserName = host.UserName;
                existing.AuthMode = host.AuthMode;
                existing.Password = host.Password;
                existing.PrivateKeyRawData = host.PrivateKeyRawData;
                existing.Passphrase = host.Passphrase;
                existing.Description = host.Description;
                existing.Features = host.Features;
                storeContext.Update(existing);
            }
            else
            {
                storeContext.Add(host);
            }
        }
        public void Delete(SshHostProfile host)
        {
            if (host == null) throw new ArgumentNullException(nameof(host));
            storeContext.Remove(host);
        }
        public bool Exist(string name)
        {
            return storeContext.GetSet<SshHostProfile>().Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        public bool Exist(Guid id, string name)
        {
            return storeContext.GetSet<SshHostProfile>().Any(x => x.Id != id && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
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
