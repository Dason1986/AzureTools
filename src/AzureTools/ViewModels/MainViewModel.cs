using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.Stores;
using AzureTools.Views;
using AzureTools.Views.AppContainers;
using AzureTools.Views.ContainerRegistries;
using AzureTools.Views.KeyVaults;
using AzureTools.Views.Options;
using AzureTools.Views.Projects;
using AzureTools.Views.Tenants;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AzureTools.ViewModels
{
    public partial class MainViewModel : ViewModel
    {
        #region Constructors

        public MainViewModel(IAzureToolsStore store, ILogger<MainViewModel> logger)
        {
            _store = store;
            _logger = logger;
            LoadMenus();
        }

        #endregion

        #region Fields & Properties

        private readonly IAzureToolsStore _store;
        private readonly ILogger<MainViewModel> _logger;

        public ObservableCollection<MenuItemCard> MenuItems { get; } = new();

        #endregion
        MenuItem[] GetMenus()
        {
            List<MenuItem> menus = new List<MenuItem>
            {
                new MenuItem { Code= "KeyVault", Name= FormResources.MenuKeyVault, Icons = FormResources.KeyVault,Form = nameof(KeyVaultSecretMainView) },
                new MenuItem { Code= "ContainerRegistry", Name= FormResources.MenuContainerRegistry, Icons = FormResources.ContainerRegistry, Form = nameof(ContainerRegistryMainView) },
                new MenuItem { Code= "AppContainer", Name= FormResources.MenuAppContainer, Icons = FormResources.ContainerService,   Form = nameof(AppContainerMainView)},
                new MenuItem { Code= "ApplicationInsights", Name= FormResources.MenuAppInsights, Icons = FormResources.ApplicationInsights },
                new MenuItem { Code= "Database", Name= FormResources.MenuDatabase, Icons = FormResources.Database },
                new MenuItem { Code= "Security", Name= FormResources.MenuSecurity, Icons = FormResources.Security,  },
                new MenuItem { Code= "Projects", Name= FormResources.MenuProjects, Icons = FormResources.Projects, Form = nameof(ProjectManagementView) },
                new MenuItem { Code= "Options", Name= FormResources.MenuOptions, Icons = FormResources.Options, Form = nameof(OptionsView) }
            };

            AppProfile profile = _store.GetProfile();
            if (profile != null && profile.TenantMode == TenantMode.Multiple)
            {
                menus.Add(new MenuItem { Code = "TenantManagement", Name = FormResources.MenuTenantManagement, Icons = FormResources.TenantManagement, Form = nameof(TenantManagementView) });
            }

            return menus.ToArray();
        }
        private void LoadMenus()
        {
            MenuItem[] menus = GetMenus();
            MenuItems.Clear();
            foreach (MenuItem menu in menus)
            {
                MenuItems.Add(new MenuItemCard
                {
                    Menu = menu,
                    ClickCommand = new RelayCommand(() => OpenMenu(menu))
                });
            }
        }

        private void OpenMenu(MenuItem menu)
        {
            if (string.IsNullOrEmpty(menu.Form)) return;
            ViewModelContext childContext = CreateChildContext();
            View view = CreateView(childContext.ServiceProvider, menu.Form);
            if (view == null)
            {
                _logger.LogWarning("未找到视图类型: {FormName}", menu.Form);
                childContext?.Dispose();
                return;
            }
            Show(view);
        }

        private static View CreateView(IServiceProvider serviceProvider, string viewName)
        {
            return viewName switch
            {
                "KeyVaultSecretMainView" => serviceProvider.GetService<KeyVaultSecretMainView>(),
                "ContainerRegistryMainView" => serviceProvider.GetService<ContainerRegistryMainView>(),
                "AppContainerMainView" => serviceProvider.GetService<AppContainerMainView>(),
                "TenantManagementView" => serviceProvider.GetService<TenantManagementView>(),
                "ProjectManagementView" => serviceProvider.GetService<ProjectManagementView>(),
                "OptionsView" => serviceProvider.GetService<OptionsView>(),
                _ => null
            };
        }
    }
}
