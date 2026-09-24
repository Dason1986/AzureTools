
using AzureTools;
using Microsoft.Extensions.DependencyInjection;
using Toolkit.Modules.Azure.KeyVaults;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Azure.ViewModels.AppContainers;
using Toolkit.Modules.Azure.ViewModels.ContainerRegistries;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;
using Toolkit.Modules.Azure.ViewModels.Tenants;
using Toolkit.Modules.Azure.Views.AppContainers;
using Toolkit.Modules.Azure.Views.ContainerRegistries;
using Toolkit.Modules.Azure.Views.KeyVaults;
using Toolkit.Modules.Azure.Views.Tenants;
using Toolkit.Modules.Properties;

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
            moduleMenu.AddMenu(code: "KeyVault", name: FormResources.MenuKeyVault, icon: FormResources.KeyVault, viewModelType: typeof(KeyVaultSecretMainViewModel));
            moduleMenu.AddMenu(code: "ContainerRegistry", name: FormResources.MenuContainerRegistry, icon: FormResources.ContainerRegistry, viewModelType: typeof(ContainerRegistryMainViewModel));
            moduleMenu.AddMenu(code: "AppContainer", name: FormResources.MenuAppContainer, icon: FormResources.ContainerService, viewModelType: typeof(AppContainerMainViewModel));
            moduleMenu.AddMenu(code: "ApplicationInsights", name: FormResources.MenuAppInsights, icon: FormResources.ApplicationInsights, viewModelType: null);
            moduleMenu.AddMenu(code: "Database", name: FormResources.MenuDatabase, icon: FormResources.Database, viewModelType: null);
            moduleMenu.AddMenu(code: "Security", name: FormResources.MenuSecurity, icon: FormResources.Security, viewModelType: null);
            moduleMenu.AddMenu(code: "TenantManagement", name: FormResources.MenuTenantManagement, icon: FormResources.TenantManagement, viewModelType: typeof(TenantManagementViewModel));
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
