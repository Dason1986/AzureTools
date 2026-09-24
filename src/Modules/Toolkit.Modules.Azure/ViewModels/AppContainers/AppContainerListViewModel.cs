using AzureTools.SDK.AppContainers;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Azure.ViewModels.AppContainers
{
    public partial class AppContainerListViewModel : ViewModel
    {
        #region Constructors

        public AppContainerListViewModel(AppContainerEnv option, ContainerAppInfo[] containerApps, IAppContainerManagement management, IDialogService dialogService, ILogger<AppContainerListViewModel> logger)
        {
            _option = option;
            _management = management;
            _dialogService = dialogService;
            _logger = logger;
            ContainerApps = containerApps == null || containerApps.Length == 0
                ? new ObservableCollection<ContainerAppInfo>()
                : new ObservableCollection<ContainerAppInfo>(containerApps.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase));
            SelectedContainerApp = ContainerApps.FirstOrDefault();
        }

        #endregion

        #region Fields & Properties

        private readonly AppContainerEnv _option;
        private readonly IAppContainerManagement _management;
        private readonly IDialogService _dialogService;
        private readonly ILogger<AppContainerListViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<ContainerAppInfo> _containerApps;

        [ObservableProperty]
        private ContainerAppInfo _selectedContainerApp;

        public string WindowTitle => string.Format(FormResources.TitleContainerAppWindow, _option?.Name);

        #endregion

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                ContainerAppInfo[] apps = await _management.GetContainerAppsAsync();
                ContainerApps = apps == null || apps.Length == 0
                    ? new ObservableCollection<ContainerAppInfo>()
                    : new ObservableCollection<ContainerAppInfo>(apps.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase));
                SelectedContainerApp = ContainerApps.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新 Container Apps 列表失败：{Name}", _option?.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task ViewDetailAsync()
        {
            if (SelectedContainerApp == null) return;
            AppContainerDetailOption option = new AppContainerDetailOption
            {
                ContainerApp = SelectedContainerApp,
                Management = _management,
                EnvOption = _option

            };
            await ShowDialog<AppContainerDetailViewModel>(option);
        }
    }
}
