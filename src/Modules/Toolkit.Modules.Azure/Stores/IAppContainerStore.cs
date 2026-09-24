using System.Collections.Generic;
using Toolkit.Modules.Azure.Models;

namespace Toolkit.Modules.Azure.Stores
{
    public interface IAppContainerStore
    {
        List<AppContainerEnv> GetOptions();
        AppContainerEnv GetOption(string name);
        void AddOption(AppContainerEnv item);
        void UpdateOption(AppContainerEnv item);
        void RemoveOption(AppContainerEnv item);
        void Save();
        void DiscardChanges();
    }
}
