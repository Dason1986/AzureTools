using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public interface IContainerRegistrieClient
    {
        string RegistryName { get; }

        Task<CliResult> AddTagAsync(string repositoryName, string digest, string newTagName, CancellationToken ct = default);
        Task<CliResult> DeleteTagAsync(string repositoryName, string tagName, CancellationToken ct = default);
    }
}
