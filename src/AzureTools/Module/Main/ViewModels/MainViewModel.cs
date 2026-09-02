using AzureTools.Models;
using AzureTools.Module.Azure.ViewModels.AppContainers;
using AzureTools.Module.Azure.ViewModels.ContainerRegistries;
using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.Module.Main.ViewModels.Options;
using AzureTools.Module.Main.ViewModels.Projects;
using AzureTools.Module.Main.ViewModels.Tenants;
using AzureTools.Properties;
using AzureTools.Stores;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Main.ViewModels
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

        private static readonly ModuleItem AzureModule = new() { Id = Guid.NewGuid(), Code = "Azure", Name = FormResources.ModuleAzure };
        private static readonly ModuleItem MainModule = new() { Id = Guid.NewGuid(), Code = "Main", Name = FormResources.ModuleMain };

        private readonly IAzureToolsStore _store;
        private readonly ILogger<MainViewModel> _logger;

        public ObservableCollection<MenuModule> Modules { get; } = new();

        [ObservableProperty]
        private MenuModule _selectedModule;

        public string CurrentModuleName => SelectedModule?.Name ?? string.Empty;

        partial void OnSelectedModuleChanged(MenuModule value)
        {
            OnPropertyChanged(nameof(CurrentModuleName));
        }

        #endregion

        private MenuItem[] GetMenus()
        {
            return new[]
            {
                new MenuItem { Code = "KeyVault", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuKeyVault, Icons = FormResources.KeyVault, ViewModelType = typeof(KeyVaultSecretMainViewModel) },
                new MenuItem { Code = "ContainerRegistry", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuContainerRegistry, Icons = FormResources.ContainerRegistry, ViewModelType = typeof(ContainerRegistryMainViewModel) },
                new MenuItem { Code = "AppContainer", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuAppContainer, Icons = FormResources.ContainerService, ViewModelType = typeof(AppContainerMainViewModel) },
                new MenuItem { Code = "ApplicationInsights", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuAppInsights, Icons = FormResources.ApplicationInsights },
                new MenuItem { Code = "Database", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuDatabase, Icons = FormResources.Database },
                new MenuItem { Code = "Security", ModuleId = AzureModule.Id, Module = AzureModule, Name = FormResources.MenuSecurity, Icons = FormResources.Security },
                new MenuItem { Code = "Projects", ModuleId = MainModule.Id, Module = MainModule, Name = FormResources.MenuProjects, Icons = FormResources.Projects, ViewModelType = typeof(ProjectManagementViewModel) },
                new MenuItem { Code = "Options", ModuleId = MainModule.Id, Module = MainModule, Name = FormResources.MenuOptions, Icons = FormResources.Options, ViewModelType = typeof(OptionsViewModel) },
                new MenuItem { Code = "TenantManagement", ModuleId = MainModule.Id, Module = MainModule, Name = FormResources.MenuTenantManagement, Icons = FormResources.TenantManagement, ViewModelType = typeof(TenantManagementViewModel) }
            };
        }

        private MenuItemCard CreateCard(MenuItem menu)
        {
            return new MenuItemCard
            {
                Menu = menu,
                ClickCommand = new RelayCommand(async () => await OpenMenu(menu))
            };
        }

        private void LoadMenus()
        {
            Modules.Clear();
            IEnumerable<IGrouping<ModuleItem, MenuItem>> groups = GetMenus().GroupBy(menu => menu.Module);
            foreach (IGrouping<ModuleItem, MenuItem> group in groups)
            {
                Modules.Add(new MenuModule
                {
                    Code = group.Key.Code,
                    Name = group.Key.Name,
                    Cards = group.Select(CreateCard).ToArray()
                });
            }

            SelectedModule = Modules.FirstOrDefault();
        }

        private async Task OpenMenu(MenuItem menu)
        {
            if (menu.ViewModelType == null) return;
            await ShowDialog(menu.ViewModelType);
        }
    }
}
