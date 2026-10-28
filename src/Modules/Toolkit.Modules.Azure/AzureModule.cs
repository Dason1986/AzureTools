
using AzureTools;
using Microsoft.Extensions.DependencyInjection;
using Toolkit.Modules.Azure.KeyVaults;
using Toolkit.Modules.Azure.Properties;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Azure.ViewModels.AppContainers;
using Toolkit.Modules.Azure.ViewModels.ContainerRegistries;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;
using Toolkit.Modules.Azure.ViewModels.Tenants;
using Toolkit.Modules.Azure.Views.AppContainers;
using Toolkit.Modules.Azure.Views.ContainerRegistries;
using Toolkit.Modules.Azure.Views.KeyVaults;
using Toolkit.Modules.Azure.Views.Tenants;

namespace Toolkit.Modules.Azure
{
    public sealed class AzureModule : ModuleBase
    {
        public AzureModule(AvaloniaComponentsOptions options) : base(options)
        {
        }

        protected override void MapViews()
        {
            MapView<AppContainerMainView, AppContainerMainViewModel>();
            MapView<AppContainerListView, AppContainerListViewModel>();
            MapView<AppContainerDetailView, AppContainerDetailViewModel>();
            MapView<AppContainerEnvManageView, AppContainerEnvManageViewModel>();
            MapView<AppContainerEnvEditorView, AppContainerEnvEditorViewModel>();
            MapView<ContainerAppReplicaDetailView, ContainerAppReplicaDetailViewModel>();

            MapView<ContainerRegistryMainView, ContainerRegistryMainViewModel>();
            MapView<ContainerRegistryEnvEditorView, ContainerRegistryEnvEditorViewModel>();
            MapView<ContainerRegistryEnvManageView, ContainerRegistryEnvManageViewModel>();
            MapView<ContainerRegistryRepositoryListView, ContainerRegistryRepositoryListViewModel>();
            MapView<ContainerRegistryManifestDetailView, ContainerRegistryManifestDetailViewModel>();

            MapView<KeyVaultSecretMainView, KeyVaultSecretMainViewModel>();
            MapView<KeyVaultEnvEditorView, KeyVaultEnvEditorViewModel>();
            MapView<KeyVaultEnvManageView, KeyVaultEnvManageViewModel>();
            MapView<KeyVaultSecretView, KeyVaultSecretViewModel>();
            MapView<KeyVaultSecretCompareView, KeyVaultSecretCompareViewModel>();
            MapView<KeyVaultSecretHistoriesView, KeyVaultSecretHistoriesViewModel>();
            MapView<KeyVaultSecretRecycleView, KeyVaultSecretRecycleViewModel>();
            MapView<SecretKeyManageView, SecretKeyManageViewModel>();
            MapView<SecretKeyEditorView, SecretKeyEditorViewModel>();

            MapView<TenantManagementView, TenantManagementViewModel>();
            MapView<TenantEditorView, TenantEditorViewModel>();



        }
        protected override void RegisterMenus()
        {
            IModuleMenuBuild moduleMenu = AddModuleMenu("Azure", "Azure");
            moduleMenu.AddMenu(code: "KeyVault", name: AzureResource.MenuKeyVault, icon: ImageResource.KeyVault, viewModelType: typeof(KeyVaultSecretMainViewModel));
            moduleMenu.AddMenu(code: "ContainerRegistry", name: AzureResource.MenuContainerRegistry, icon: ImageResource.ContainerRegistry, viewModelType: typeof(ContainerRegistryMainViewModel));
            moduleMenu.AddMenu(code: "AppContainer", name: AzureResource.MenuAppContainer, icon: ImageResource.ContainerService, viewModelType: typeof(AppContainerMainViewModel));
            moduleMenu.AddMenu(code: "ApplicationInsights", name: AzureResource.MenuAppInsights, icon: ImageResource.ApplicationInsights, viewModelType: null);
            moduleMenu.AddMenu(code: "Database", name: AzureResource.MenuDatabase, icon: ImageResource.Database, viewModelType: null);
            moduleMenu.AddMenu(code: "Security", name: AzureResource.MenuSecurity, icon: ImageResource.Security, viewModelType: null);
            moduleMenu.AddMenu(code: "TenantManagement", name: AzureResource.MenuTenantManagement, icon: ImageResource.TenantManagement, viewModelType: typeof(TenantManagementViewModel));
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IKeyVaultStore, DefaultKeyVaultsStore>();
            services.AddSingleton<IContainerRegistryStore, DefaultContainerRegistryStore>();
            services.AddSingleton<IAppContainerStore, DefaultAppContainerStore>();
            services.AddSingleton<IKeyVaultExportFiles, KeyVaultExportFiles>();
            services.AddTransient<IKeyVaultCompareService, KeyVaultCompareService>();

            services.AddScoped<ITenantProvider, TenantProvider>();
        }
    }
}
