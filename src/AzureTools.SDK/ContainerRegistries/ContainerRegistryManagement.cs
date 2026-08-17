using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Azure.Identity;
using AzureTools.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.SDK.ContainerRegistries
{

    class AzureCliClientFactory
    {
        public bool CliExist()
        {
            return false;
        }

        public Task<string> Get(string cmd )
        {
            return Task.Run<string>(() =>
            {
                var info = new ProcessStartInfo
                {
                    FileName = "az",
                    Arguments = cmd,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = Process.Start(info);
                if (process == null)
                    return  null;
                var tcs = new TaskCompletionSource<bool>();
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        errorBuilder.AppendLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

            
                return outputBuilder.ToString();
            });

        }
    }
    public class ContainerRegistryManagement : IContainerRegistryManagement
    {
        public ContainerRegistryManagement(string registryName, string registryUrl, TokenCredential credential, ContainerRegistryClient client = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(registryName);
            ArgumentException.ThrowIfNullOrEmpty(registryUrl);
            if (!Uri.TryCreate(registryUrl, UriKind.RelativeOrAbsolute, out Uri endpoint)) throw new ArgumentException(registryUrl);
            RegistryName = registryName;
            this.client = client ?? new ContainerRegistryClient(endpoint, credential);
            isCli = credential is AzureCliCredential;

        }
        private readonly bool isCli;

        public ContainerRegistryManagement(string registryName, TokenCredential credential, ContainerRegistryClient client = null) : this(registryName, $"https://{registryName}.azurecr.io", credential, client)
        {
        }

        private readonly ContainerRegistryClient client;

        public string RegistryName { get; }

        public bool SupportManifestDetail => isCli;

        public bool SupportRename => isCli;

        public bool SupportFixedManifest => true;

        public async Task FixedManifest(string repositoryName, string digest, bool isFixed)
        {
            ContainerRepository repo = client.GetRepository(repositoryName);
            RegistryArtifact artifact = repo.GetArtifact(digest);
            await artifact.UpdateManifestPropertiesAsync(new ArtifactManifestProperties { CanDelete = !isFixed });
        }

        public async Task<RepositoryInfo[]> GetAllRepositories()
        {
            List<RepositoryInfo> repos = new List<RepositoryInfo>();

            await foreach (string repoName in client.GetRepositoryNamesAsync())
            {
                ContainerRepository repo = client.GetRepository(repoName);

                repos.Add(new RepositoryInfo
                {
                    Name = repo.Name,
                    RegistryEndpoint = repo.RegistryEndpoint.ToString(),
                    ManifestProperties = repo.GetAllManifestProperties().Select(x => new ManifestProperty
                    {
                        RepositoryName = repo.Name,
                        CreatedOn = x.CreatedOn.ToLocalDateTime(),
                        CanDelete = x.CanDelete.GetValueOrDefault(),
                        Architecture = $"{x.OperatingSystem} {x.Architecture}",
                        LastUpdatedOn = x.LastUpdatedOn.ToLocalDateTime(),
                        Digest = x.Digest,
                        SizeInBytes = x.SizeInBytes,
                        Tags = x.Tags == null ? Array.Empty<string>() : x.Tags.ToArray(),
                    }).OrderByDescending(x => x.CreatedOn).ToArray(),
                });
            }
            return repos.ToArray();
        }

        public async Task DeleteTag(string repositoryName, string digest, string tagName)
        {
            RegistryArtifact artifact = client.GetArtifact(repositoryName, digest);
            if (artifact == null) return;
            await artifact.DeleteTagAsync(tagName);
        }

        public async Task DeleteManifest(string repositoryName, string digest)
        {
            RegistryArtifact artifact = client.GetArtifact(repositoryName, digest);
            if (artifact == null) return;
            await artifact.DeleteAsync();
        }

        Task<ManifestDetail> IContainerRegistryManagement.GetManifestDetailAsync(string repositoryName, string digest)
        {
            throw new NotSupportedException();
        }

        Task IContainerRegistryManagement.RenameTag(string repositoryName, string digest, string oldTagName, string newName)
        {
            throw new NotSupportedException();
        }
    }
}
