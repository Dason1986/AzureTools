using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.Resources;
using AzureTools.Cli;
using AzureTools.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            SupportContainerLogs = credential is AzureCliCredential;

        }

        private readonly AppContainerAzureEnv _option;
        private readonly TokenCredential _credential;

        public string EnvironmentName { get; }

        public bool SupportEnterContainer => SupportContainerLogs;

        public bool SupportContainerLogs { get; }

        public async Task<ContainerAppInfo[]> GetContainerAppsAsync(CancellationToken ct = default)
        {
            ArmClient armClient = new ArmClient(_credential);
            SubscriptionResource subscription = GetSubscription(armClient);
            if (subscription == null) return Array.Empty<ContainerAppInfo>();


            List<ContainerAppInfo> apps = new List<ContainerAppInfo>();
            ContainerAppBuilder appBuilder = new ContainerAppBuilder();
            await foreach (ContainerAppResource app in subscription.GetContainerAppsAsync(ct))
            {
                ContainerAppData data = app.Data;
                if (data.EnvironmentId?.Name == _option.EnvironmentName)
                {
                    apps.Add(appBuilder.BuildAppInfo(data));
                }
            }

            return apps.OrderBy(x => x.Name).ToArray();
        }

        public async Task<ContainerAppDetail> GetContainerAppDetailAsync(string containerAppName, CancellationToken ct = default)
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


            ContainerAppBuilder appBuilder = new ContainerAppBuilder();

            return await appBuilder.BuildDetail(subscription, containerAppName, _option.EnvironmentName, ct);
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

        public async Task<ICliStreamSession> GetContainerLogsFollowAsync(string containerAppName, string resourceGroup, string revision, string replica, CancellationToken ct = default)
        {
            if (!SupportContainerLogs) throw new NotSupportedException();
            IAzureCliClientBuilder builder = await AzureCliBuilder.CreateAzureAsync();
            await AzureCliBuilder.ChangeSubscription(this._option.SubscriptionId);
            return await builder.GetAppContainerClient(containerAppName, resourceGroup).GetLogsFollowAsync(revision, replica, ct);

        }

        public async Task<ICliStreamSession> ExecContainerAsync(string containerAppName, string resourceGroup, string revision, string replica, string containerName, CancellationToken ct = default)
        {
            if (!SupportEnterContainer) throw new NotSupportedException();
            await AzureCliBuilder.ChangeSubscription(this._option.SubscriptionId);
            IAzureCliClientBuilder builder = await AzureCliBuilder.CreateAzureAsync();
            return await builder.GetAppContainerClient(containerAppName, resourceGroup).ExecAsync(revision, replica, containerName, ct);

        }

        public async Task<IContainerFileProvider> GetContainerFileBrowserAsync(string containerAppName, string resourceGroup, string revision, string replica, string containerName, CancellationToken ct = default)
        {
            if (!SupportEnterContainer) throw new NotSupportedException();
            await AzureCliBuilder.ChangeSubscription(this._option.SubscriptionId);
            IAzureCliClientBuilder builder = await AzureCliBuilder.CreateAzureAsync();
            return await builder.GetAppContainerClient(containerAppName, resourceGroup).GetFileBrowserAsync(revision, replica, containerName, ct);

        }
        public async Task RevisionRestartAsync(string containerAppName, string resourceGroup, string revision, CancellationToken ct = default)
        {

            if (!SupportContainerLogs) throw new NotSupportedException();
            await AzureCliBuilder.ChangeSubscription(this._option.SubscriptionId);
            IAzureCliClientBuilder builder = await AzureCliBuilder.CreateAzureAsync();
            await builder.GetAppContainerClient(containerAppName, resourceGroup).RevisionRestartAsync(revision, ct);
        }

    }
}
