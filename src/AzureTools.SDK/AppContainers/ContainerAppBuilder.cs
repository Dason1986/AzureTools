using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.Resources;
using AzureTools.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.SDK.AppContainers
{
    class ContainerAppBuilder
    {

        public async Task<ContainerAppDetail> BuildDetail(SubscriptionResource subscription, string containerAppName, string environmentName, CancellationToken ct = default)
        {
            ContainerAppResource resource = await FindContainerAppResourceAsync(subscription, containerAppName, environmentName, ct);
            if (resource == null)
            {
                return null;
            }
            ContainerAppData data = resource.Data;
            IDictionary<string, string> secrets = await GetSecretsAsync(resource, ct);
            ContainerAppDetail detail = new ContainerAppDetail
            {
                Name = data.Name,
                EnvironmentName = data.EnvironmentId?.Name,
                ProvisioningState = data.ProvisioningState.ToString(),
                RunningStatus = data.RunningStatus?.ToString(),
                Location = data.Location,
                LatestRevisionName = data.LatestRevisionName,
                LatestReadyRevisionName = data.LatestReadyRevisionName,
                LatestRevisionFqdn = data.LatestRevisionFqdn,
                LastModifiedTime = data.SystemData.LastModifiedOn.ToLocalDateTime(),
                CreatedTime = data.SystemData.CreatedOn.ToLocalDateTime(),
                Scale = BuildScaleInfo(data.Template?.Scale),
                Volumes = BuildVolumeInfos(data.Template?.Volumes),
                EnvironmentVariables = BuildEnvVarInfos(data.Template?.Containers, secrets),
                Ingress = BuildIngress(data.Configuration.Ingress)
            };

            await PopulateRevisionsAsync(resource, detail);
            return detail;
        }
        public virtual ContainerAppInfo BuildAppInfo(ContainerAppData data)
        {
            return new ContainerAppInfo
            {
                Name = data.Name,
                EnvironmentName = data.EnvironmentId?.Name,
                ProvisioningState = data.ProvisioningState.ToString(),
                RunningStatus = data.RunningStatus?.ToString(),
                LatestRevisionName = data.LatestRevisionName,
                Replicas = data.Template?.Scale?.MaxReplicas,
                Image = data.Template?.Containers?.FirstOrDefault()?.Image,
                Location = data.Location,
                ResourceType = data.ResourceType.ToString(),
                RevisionCreatedTime = data.LatestRevisionFqdn != null ? DateTime.UtcNow : null
            };
        }
        #region Cast
        private async Task<ContainerAppResource> FindContainerAppResourceAsync(SubscriptionResource subscription, string containerAppName, string environmentName, CancellationToken ct = default)
        {
            await foreach (ContainerAppResource app in subscription.GetContainerAppsAsync(ct))
            {
                if (app.Data.Name == containerAppName && app.Data.EnvironmentId?.Name == environmentName)
                {
                    return app;
                }
            }
            return null;
        }
        protected virtual async Task<IDictionary<string, string>> GetSecretsAsync(ContainerAppResource resource, CancellationToken ct = default)
        {
            IDictionary<string, string> secret = new Dictionary<string, string>();
            await foreach (ContainerAppSecret app in resource.GetSecretsAsync(ct))
            {
                secret.Add(app.Name, app.Value);
            }
            return secret;
        }
        private ContainerAppIngress BuildIngress(ContainerAppIngressConfiguration ingress)
        {
            return new ContainerAppIngress
            {
                Transport = ingress.Transport,
                External = ingress.External,
                ExposedPort = ingress.ExposedPort,
            };
        }



        protected virtual async Task PopulateRevisionsAsync(ContainerAppResource appResource, ContainerAppDetail detail, CancellationToken ct = default)
        {
            ContainerAppRevisionCollection revisionCollection = appResource.GetContainerAppRevisions();
            await foreach (ContainerAppRevisionResource revision in revisionCollection.GetAllAsync(null, ct))
            {
                ContainerAppRevisionData revData = revision.Data;
                detail.Revisions.Add(new ContainerAppRevisionInfo
                {
                    Name = revData.Name,
                    ProvisioningState = revData.ProvisioningState?.ToString(),
                    RunningState = revData.RunningState?.ToString(),
                    HealthState = revData.HealthState?.ToString(),
                    IsActive = revData.IsActive.GetValueOrDefault(),
                    Replicas = revData.Replicas,
                    TrafficWeight = revData.TrafficWeight,
                    CreatedOn = revData.CreatedOn.ToLocalDateTime(),
                    LastActiveOn = revData.LastActiveOn.ToLocalDateTime(),
                    Fqdn = revData.Fqdn
                });

                if (revData.IsActive.GetValueOrDefault())
                {
                    await PopulateReplicasAsync(revision, revData.Name, detail);
                }
            }
            detail.Revisions = detail.Revisions.OrderByDescending(x => x.CreatedOn).ToList();
        }

        protected virtual async Task PopulateReplicasAsync(ContainerAppRevisionResource revision, string revisionName, ContainerAppDetail detail, CancellationToken ct = default)
        {
            ContainerAppReplicaCollection replicaCollection = revision.GetContainerAppReplicas();
            await foreach (ContainerAppReplicaResource replica in replicaCollection.GetAllAsync(ct))
            {
                detail.Replicas.Add(BuildReplicaInfo(replica.Data, revisionName));
            }
        }

        protected virtual ContainerAppReplicaInfo BuildReplicaInfo(ContainerAppReplicaData data, string revisionName)
        {
            ContainerAppReplicaInfo info = new ContainerAppReplicaInfo
            {
                Name = data.Name,
                RevisionName = revisionName,
                CreatedOn = data.CreatedOn.ToLocalDateTime(),
                RunningState = data.RunningState?.ToString(),
                RunningStateDetails = data.RunningStateDetails
            };

            if (data.Containers != null)
            {
                foreach (ContainerAppReplicaContainer container in data.Containers)
                {
                    info.Containers.Add(new ContainerAppReplicaContainerInfo
                    {
                        Name = container.Name,
                        ContainerId = container.ContainerId,
                        IsReady = container.IsReady,
                        IsStarted = container.IsStarted,
                        RestartCount = container.RestartCount,
                        RunningState = container.RunningState?.ToString(),
                        RunningStateDetails = container.RunningStateDetails
                    });
                }
            }

            return info;
        }



        protected virtual ContainerAppScaleInfo BuildScaleInfo(ContainerAppScale scale)
        {
            if (scale == null) return null;

            ContainerAppScaleInfo scaleInfo = new ContainerAppScaleInfo
            {
                MinReplicas = scale.MinReplicas,
                MaxReplicas = scale.MaxReplicas,
                CooldownPeriod = scale.CooldownPeriod?.ToString(),
                PollingInterval = scale.PollingInterval?.ToString()
            };

            if (scale.Rules != null)
            {
                foreach (ContainerAppScaleRule rule in scale.Rules)
                {
                    scaleInfo.ScaleRules.Add(rule.Name);
                }
            }

            return scaleInfo;
        }

        protected virtual List<ContainerAppVolumeInfo> BuildVolumeInfos(IList<ContainerAppVolume> volumes)
        {
            List<ContainerAppVolumeInfo> result = new List<ContainerAppVolumeInfo>();
            if (volumes == null) return result;

            foreach (ContainerAppVolume volume in volumes)
            {
                result.Add(new ContainerAppVolumeInfo
                {
                    Name = volume.Name,
                    StorageType = volume.StorageType?.ToString(),
                    StorageName = volume.StorageName,
                    MountOptions = volume.MountOptions
                });
            }

            return result;
        }

        protected virtual List<ContainerAppEnvVarInfo> BuildEnvVarInfos(IList<ContainerAppContainer> containers, IDictionary<string, string> secrets)
        {
            List<ContainerAppEnvVarInfo> result = new List<ContainerAppEnvVarInfo>();
            if (containers == null) return result;

            foreach (ContainerAppContainer container in containers)
            {
                if (container.Env == null) continue;

                foreach (ContainerAppEnvironmentVariable envVar in container.Env)
                {
                    result.Add(new ContainerAppEnvVarInfo
                    {
                        Name = envVar.Name,
                        Value = !string.IsNullOrEmpty(envVar.SecretRef) && secrets.TryGetValue(envVar.SecretRef, out string val) ? val : envVar.Value,
                        SecretRef = envVar.SecretRef
                    });
                }
            }

            return result;
        }



        #endregion
    }
}
