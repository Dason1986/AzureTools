using System.Threading.Tasks;

namespace AzureTools.SDK.ContainerRegistries
{
    public interface IContainerRegistryManagement
    {
        string RegistryName { get; }

        bool SupportManifestDetail { get; }
        bool SupportRename { get; }
        bool SupportFixedManifest { get; }

        Task DeleteManifestAsync(string repositoryName, string digest);
        Task FixedManifestAsync(string repositoryName, string digest, bool isFixed);
        Task DeleteTagAsync(string repositoryName, string digest, string tagName);
        Task AddTagAsync(string repositoryName, string digest, string newName);
        Task RenameTagAsync(string repositoryName, string digest, string oldTagName, string newName);
        Task<RepositoryInfo[]> GetAllRepositoriesAsync();
        Task<ManifestDetail> GetManifestDetailAsync(string repositoryName, string digest);
    }
}
