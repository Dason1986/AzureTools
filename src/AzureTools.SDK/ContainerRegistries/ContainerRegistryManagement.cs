using Azure.Containers.ContainerRegistry;
using Azure.Core;
using Azure.Identity;
using AzureTools.Cli;
using AzureTools.Extensions;
using AzureTools.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public Task<string> Get(string cmd)
        {
            return Task.Run<string>(() =>
            {
                ProcessStartInfo info = new ProcessStartInfo
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

                using Process process = Process.Start(info);
                if (process == null)
                    return null;
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                StringBuilder outputBuilder = new StringBuilder();
                StringBuilder errorBuilder = new StringBuilder();

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

        public ContainerRegistryManagement(ContainerRegistryAzureEnv azureEnv, TokenCredential credential)
        {
            string registryName = azureEnv.Registry;
            if (registryName.EndsWith(".azurecr.io")) registryName = registryName.Replace(".azurecr.io", string.Empty);
            RegistryName = registryName;
            string registryUrl = string.IsNullOrEmpty(azureEnv.RegistryUrl) ? $"https://{registryName}.azurecr.io" : azureEnv.RegistryUrl;

            if (!Uri.TryCreate(registryUrl, UriKind.RelativeOrAbsolute, out Uri endpoint)) throw new ArgumentException(registryUrl);
            this.client = client ?? new ContainerRegistryClient(endpoint, credential);
            SupportRename = credential is AzureCliCredential;
            this.azureEnv = azureEnv;
        }

        private readonly ContainerRegistryClient client;
        private readonly ContainerRegistryAzureEnv azureEnv;

        public string RegistryName { get; }
        public string RegistryUrl { get; }
        public bool SupportManifestDetail { get; }

        public bool SupportRename { get; }

        public bool SupportFixedManifest => true;

        public async Task FixedManifestAsync(string repositoryName, string digest, bool isFixed)
        {
            ContainerRepository repo = client.GetRepository(repositoryName);
            RegistryArtifact artifact = repo.GetArtifact(digest);
            await artifact.UpdateManifestPropertiesAsync(new ArtifactManifestProperties { CanDelete = !isFixed });
        }

        public async Task<RepositoryInfo[]> GetAllRepositoriesAsync()
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

        public async Task DeleteTagAsync(string repositoryName, string digest, string tagName)
        {
            RegistryArtifact artifact = client.GetArtifact(repositoryName, digest);
            if (artifact == null) throw new AzureToolsException($"{repositoryName}:{digest} not exist");
            await artifact.DeleteTagAsync(tagName);
        }

        public async Task DeleteManifestAsync(string repositoryName, string digest)
        {
            RegistryArtifact artifact = client.GetArtifact(repositoryName, digest);
            if (artifact == null) throw new AzureToolsException($"{repositoryName}:{digest} not exist");
            await artifact.DeleteAsync();
        }

        Task<ManifestDetail> IContainerRegistryManagement.GetManifestDetailAsync(string repositoryName, string digest)
        {
            throw new NotSupportedException();
        }

        public async Task AddTagAsync(string repositoryName, string digest, string tagName)
        {
            if (!SupportRename) throw new NotSupportedException();

            await AddOrDeleteTag(repositoryName, digest, null, tagName);
        }
        public async Task RenameTagAsync(string repositoryName, string digest, string oldTagName, string newTagName)
        {
            if (!SupportRename) throw new NotSupportedException();
            await AddOrDeleteTag(repositoryName, digest, oldTagName, newTagName);
        }

        async Task AddOrDeleteTag(string repositoryName, string digest, string oldTagName, string newTagName, CancellationToken ct = default)
        {
            await AzureCliBuilder.ChangeSubscription(this.azureEnv.SubscriptionId);
            RegistryArtifact artifact = client.GetArtifact(repositoryName, digest);
            if (artifact == null) throw new AzureToolsException($"{repositoryName}:{digest} not exist");
            IAzureCliClientBuilder builder = await AzureCliBuilder.CreateAzureAsync();
            IContainerRegistrieClient clinet = builder.GetContainerRegistrieClient(RegistryName, this.azureEnv.ResourceGroupName);
            if (!string.IsNullOrEmpty(newTagName))
            {
                CliResult result = await clinet.AddTagAsync(repositoryName, digest, newTagName, ct);
                if (!result.Success && !string.IsNullOrEmpty(result.Error))
                {
                    string oper = string.IsNullOrEmpty(oldTagName) ? "AddTag" : "RenameTag";
                    throw new AzureToolsException($"{repositoryName}:{digest} {oper} failed {result.Error}");
                }
            }
            if (!string.IsNullOrEmpty(oldTagName))
            {
                CliResult result = await clinet.DeleteTagAsync(repositoryName, oldTagName, ct);
                if (!result.Success && !string.IsNullOrEmpty(result.Error)) throw new AzureToolsException($"{repositoryName}:{digest} RenameTag failed {result.Error}");
            }
        }
    }
}
