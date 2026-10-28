using AzureTools.SDK;
using AzureTools.SDK.AppContainers;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Properties;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;


namespace Toolkit.Modules.Azure.ViewModels.AppContainers
{
    public partial class AppContainerMainViewModel : ViewModel
    {
        #region Constructors

        public AppContainerMainViewModel(IAppContainerStore appContainerStore, IToolkitStore azureToolsStore, IAzureResourceFactory azureResourceFactory, IDialogService dialogService, ILogger<AppContainerMainViewModel> logger)
        {
            _appContainerStore = appContainerStore;
            _azureToolsStore = azureToolsStore;
            _azureResourceFactory = azureResourceFactory;
            _dialogService = dialogService;
            _logger = logger;
            InitializeOptions();
        }

        #endregion

        #region Fields & Properties

        private readonly IAppContainerStore _appContainerStore;
        private readonly IToolkitStore _azureToolsStore;
        private readonly IAzureResourceFactory _azureResourceFactory;
        private readonly IDialogService _dialogService;
        private readonly ILogger<AppContainerMainViewModel> _logger;

        [ObservableProperty]
        private List<AppContainerEnv> _appContainerOptionsList;

        [ObservableProperty]
        private AppContainerEnv _selectedAppContainerOption;

        #endregion

        public void InitializeOptions()
        {
            AppContainerOptionsList = _appContainerStore.GetOptions();
            if (AppContainerOptionsList == null || AppContainerOptionsList.Count == 0)
            {
                _ = _dialogService.ShowWarningAsync(AzureResource.NoEnvConfigured);
                return;
            }

            SelectedAppContainerOption = AppContainerOptionsList[0];
        }

        [RelayCommand]
        private async Task QueryAsync()
        {
            if (SelectedAppContainerOption == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.SelectValidEnv);
                return;
            }

            if (string.IsNullOrEmpty(SelectedAppContainerOption.EnvironmentName))
            {
                await _dialogService.ShowInfoAsync(AzureResource.InvalidEnvConfig);
                return;
            }

            AppContainerEnv option = SelectedAppContainerOption;
            TenantProfile profile = _azureToolsStore.GetTenant(option.TenantId);
            if (profile == null)
            {
                await _dialogService.ShowInfoAsync(AzureResource.NoTenantSelected);
                return;
            }

            try
            {
                IAppContainerManagement management = _azureResourceFactory.GetAppContainerManagement(option.ToSdk(), profile.ToSdk());
                ContainerAppInfo[] containerApps = await management.GetContainerAppsAsync();
                await OpenContainerAppListAsync(option, containerApps, management);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询 Container Apps 列表失败：{Name}", option.EnvironmentName);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task EnvManageAsync()
        {
            await ShowDialog<AppContainerEnvManageViewModel>();
            InitializeOptions();
        }

        private async Task OpenContainerAppListAsync(AppContainerEnv option, ContainerAppInfo[] containerApps, IAppContainerManagement management)
        {
            await ShowDialog<AppContainerListViewModel>(option, containerApps, management);
        }
    }
}
