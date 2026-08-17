using AzureTools.Models;
using System.Collections.Generic;

namespace AzureTools.Stores
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
