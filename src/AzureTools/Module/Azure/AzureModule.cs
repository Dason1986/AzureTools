using AzureTools.Module.Azure.ViewModels.AppContainers;
using AzureTools.Module.Azure.ViewModels.ContainerRegistries;
using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.Module.Azure.Views.AppContainers;
using AzureTools.Module.Azure.Views.ContainerRegistries;
using AzureTools.Module.Azure.Views.KeyVaults;
using AzureTools.Services.KeyVaults;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTools.Module.Azure
{
    public sealed class AzureModule : ViewModuleBase
    {
        protected override void MapViews(AvaloniaComponentsOptions options)
        {
            options.MapView<AppContainerMainView, AppContainerMainViewModel>();
            options.MapView<AppContainerListView, AppContainerListViewModel>();
            options.MapView<AppContainerDetailView, AppContainerDetailViewModel>();
            options.MapView<AppContainerEnvManageView, AppContainerEnvManageViewModel>();
            options.MapView<AppContainerEnvEditorView, AppContainerEnvEditorViewModel>();
            options.MapView<ContainerAppReplicaDetailView, ContainerAppReplicaDetailViewModel>();

            options.MapView<ContainerRegistryMainView, ContainerRegistryMainViewModel>();
            options.MapView<ContainerRegistryEnvEditorView, ContainerRegistryEnvEditorViewModel>();
            options.MapView<ContainerRegistryEnvManageView, ContainerRegistryEnvManageViewModel>();
            options.MapView<ContainerRegistryRepositoryListView, ContainerRegistryRepositoryListViewModel>();
            options.MapView<ContainerRegistryManifestDetailView, ContainerRegistryManifestDetailViewModel>();

            options.MapView<KeyVaultSecretMainView, KeyVaultSecretMainViewModel>();
            options.MapView<KeyVaultEnvEditorView, KeyVaultEnvEditorViewModel>();
            options.MapView<KeyVaultEnvManageView, KeyVaultEnvManageViewModel>();
            options.MapView<KeyVaultSecretView, KeyVaultSecretViewModel>();
            options.MapView<KeyVaultSecretCompareView, KeyVaultSecretCompareViewModel>();
            options.MapView<KeyVaultSecretHistoriesView, KeyVaultSecretHistoriesViewModel>();
            options.MapView<KeyVaultSecretRecycleView, KeyVaultSecretRecycleViewModel>();
            options.MapView<SecretKeyManageView, SecretKeyManageViewModel>();
            options.MapView<SecretKeyEditorView, SecretKeyEditorViewModel>();
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IKeyVaultExportFiles, KeyVaultExportFiles>();
            services.AddTransient<IKeyVaultCompareService, KeyVaultCompareService>();
        }
    }
}
