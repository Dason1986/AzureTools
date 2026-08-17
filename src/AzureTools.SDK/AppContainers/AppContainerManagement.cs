using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.Resources;
using AzureTools.Extensions;
using AzureTools.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.SDK.AppContainers
{
    public class AppContainerManagement : IAppContainerManagement
    {
        public AppContainerManagement(AppContainerAzureEnv option, TokenCredential credential)
        {
            ArgumentNullException.ThrowIfNull(option);
            ArgumentNullException.ThrowIfNull(credential);
            _option = option;
            _credential = credential;
            EnvironmentName = option.EnvironmentName;
            isCli = credential is AzureCliCredential;

        }
        private readonly bool isCli;
        private readonly AppContainerAzureEnv _option;
        private readonly TokenCredential _credential;

        public string EnvironmentName { get; }

        public async Task<ContainerAppInfo[]> GetContainerAppsAsync()
        {
            ArmClient armClient = new ArmClient(_credential);
            SubscriptionResource subscription = GetSubscription(armClient);
            if (subscription == null)
            {
                return Array.Empty<ContainerAppInfo>();
            }

            List<ContainerAppInfo> apps = new List<ContainerAppInfo>();
            await foreach (ContainerAppResource app in subscription.GetContainerAppsAsync())
            {
                ContainerAppData data = app.Data;
                if (data.EnvironmentId?.Name == _option.EnvironmentName)
                {
                    apps.Add(BuildContainerAppInfo(data));
                }
            }

            return apps.OrderBy(x => x.Name).ToArray();
        }

        public async Task<ContainerAppDetail> GetContainerAppDetailAsync(string containerAppName)
        {
            if (string.IsNullOrWhiteSpace(containerAppName))
            {
                throw new ArgumentNullException(nameof(containerAppName));
            }

            ArmClient armClient = new ArmClient(_credential);
            SubscriptionResource subscription = GetSubscription(armClient);
            if (subscription == null)
            {
                return null;
            }

            ContainerAppResource appResource = await FindContainerAppResourceAsync(subscription, containerAppName);
            if (appResource == null)
            {
                return null;
            }

            ContainerAppData data = appResource.Data;
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
                Scale = BuildScaleInfo(data.Template?.Scale),
                Volumes = BuildVolumeInfos(data.Template?.Volumes),
                EnvironmentVariables = BuildEnvVarInfos(data.Template?.Containers)
            };

            await PopulateRevisionsAsync(appResource, detail);
            return detail;
        }

        private async Task<ContainerAppResource> FindContainerAppResourceAsync(SubscriptionResource subscription, string containerAppName)
        {
            await foreach (ContainerAppResource app in subscription.GetContainerAppsAsync())
            {
                if (app.Data.Name == containerAppName && app.Data.EnvironmentId?.Name == _option.EnvironmentName)
                {
                    return app;
                }
            }
            return null;
        }

        private static async Task PopulateRevisionsAsync(ContainerAppResource appResource, ContainerAppDetail detail)
        {
            ContainerAppRevisionCollection revisionCollection = appResource.GetContainerAppRevisions();
            await foreach (ContainerAppRevisionResource revision in revisionCollection.GetAllAsync())
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

        private static async Task PopulateReplicasAsync(ContainerAppRevisionResource revision, string revisionName, ContainerAppDetail detail)
        {
            ContainerAppReplicaCollection replicaCollection = revision.GetContainerAppReplicas();
            await foreach (ContainerAppReplicaResource replica in replicaCollection.GetAllAsync())
            {
                detail.Replicas.Add(BuildReplicaInfo(replica.Data, revisionName));
            }
        }

        private static ContainerAppReplicaInfo BuildReplicaInfo(ContainerAppReplicaData data, string revisionName)
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

        private static ContainerAppInfo BuildContainerAppInfo(ContainerAppData data)
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

        private static ContainerAppScaleInfo BuildScaleInfo(ContainerAppScale scale)
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

        private static List<ContainerAppVolumeInfo> BuildVolumeInfos(IList<ContainerAppVolume> volumes)
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

        private static List<ContainerAppEnvVarInfo> BuildEnvVarInfos(IList<ContainerAppContainer> containers)
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
                        Value = envVar.Value,
                        SecretRef = envVar.SecretRef
                    });
                }
            }

            return result;
        }

        private SubscriptionResource GetSubscription(ArmClient armClient)
        {
            foreach (SubscriptionResource subscription in armClient.GetSubscriptions())
            {
                if (subscription.Data.SubscriptionId == _option.SubscriptionId)
                {
                    return subscription;
                }
            }
            return null;
        }
    }
}
