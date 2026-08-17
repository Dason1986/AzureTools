using System.Threading.Tasks;

namespace AzureTools.SDK.ContainerRegistries
{
    public interface IContainerRegistryManagement
    {
        string RegistryName { get; }

        bool SupportManifestDetail { get; }
        bool SupportRename { get; }
        bool SupportFixedManifest { get; }

        Task DeleteManifest(string repositoryName, string digest);
        Task FixedManifest(string repositoryName, string digest, bool isFixed);
        Task DeleteTag(string repositoryName, string digest, string tagName);
        Task RenameTag(string repositoryName, string digest, string oldTagName, string newName);
        Task<RepositoryInfo[]> GetAllRepositories();
        Task<ManifestDetail> GetManifestDetailAsync(string repositoryName, string digest);
    }
}
