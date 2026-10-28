using System;
using System.Collections.Generic;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.Stores
{
    public interface ISshHostStore
    {
        List<SshHostProfile> GetHosts();
        SshHostProfile GetHost(Guid id);
        void AddOrUpdate(SshHostProfile host);
        void Delete(SshHostProfile host);
        bool Exist(string name);
        bool Exist(Guid id, string name);
        void Save();
        void DiscardChanges();
    }
}
