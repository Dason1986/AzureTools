using AzureTools.Models;
using System.Collections.Generic;

namespace AzureTools.Stores
{
    public interface IContainerRegistryStore
    {
        List<ContainerRegistryEnv> GetOptions();
        ContainerRegistryEnv GetOption(string name);
        void AddOption(ContainerRegistryEnv item);
        void UpdateOption(ContainerRegistryEnv item);
        void RemoveOption(ContainerRegistryEnv item);
        void Save();
        void DiscardChanges();
    }
}
