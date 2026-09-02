using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Cli
{
    public class ContainerRegistrieClient : IContainerRegistrieClient
    {
        #region Constructors
        public ContainerRegistrieClient(string azPath, string registryName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(registryName);
            this.azPath = azPath;
            RegistryName = registryName;
        }
        #endregion
        #region Fields & Properties
        private readonly string azPath;

        public string RegistryName { get; }
        #endregion
        public async Task<CliResult> AddTagAsync(string repositoryName, string digest, string newTagName, CancellationToken ct = default)
        {
            string parms = $"acr import --name {RegistryName} --source {RegistryName}.azurecr.io/{repositoryName}@{digest} -t {repositoryName}:{newTagName}";
            return await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
        }

        public async Task<CliResult> DeleteTagAsync(string repositoryName, string tagName, CancellationToken ct = default)
        {
            string parms = $"acr repository untag --name {RegistryName} --image {repositoryName}:{tagName}";
            return await CliClientBuilder.Create(azPath).Args(parms).ExecuteAsync(ct);
        }
    }
}
