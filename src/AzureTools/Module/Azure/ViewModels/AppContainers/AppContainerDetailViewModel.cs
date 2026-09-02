using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK.AppContainers;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.AppContainers
{
    public partial class AppContainerDetailViewModel : ViewModel
    {
        #region Constructors

        public AppContainerDetailViewModel(AppContainerDetailOption option, IDialogService dialogService, ILogger<AppContainerDetailViewModel> logger)
        {
            _containerApp = option.ContainerApp;
            _management = option.Management;
            envOption = option.EnvOption;
            _dialogService = dialogService;
            _logger = logger;
            ResourceGroupName = option.EnvOption.ResourceGroupName;
            WindowTitle = string.Format(FormResources.TitleContainerAppDetail, _containerApp?.Name);
            _ = LoadDetailAsync();
        }

        #endregion

        #region Fields & Properties

        private readonly ContainerAppInfo _containerApp;
        private readonly IAppContainerManagement _management;
        private readonly AppContainerEnv envOption;
        private readonly IDialogService _dialogService;
        private readonly ILogger<AppContainerDetailViewModel> _logger;

        public string ResourceGroupName { get; }

        [ObservableProperty]
        private ContainerAppDetail _detail;

        [ObservableProperty]
        private ObservableCollection<ContainerAppRevisionInfo> _revisions;

        [ObservableProperty]
        private ObservableCollection<ContainerAppReplicaInfo> _replicas;

        [ObservableProperty]
        private ContainerAppReplicaInfo _selectedReplica;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private ObservableCollection<ContainerAppEnvVarInfo> _filteredEnvironmentVariables = new();

        [ObservableProperty]
        private string _envVarFilterText;



        #endregion

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDetailAsync();
        }
        [RelayCommand]
        private async Task RevisionRestartAsync()
        {
            if (SelectedReplica == null) return;
            await _management.RevisionRestartAsync(_containerApp.Name, envOption.ResourceGroupName, SelectedReplica.RevisionName);
            await this._dialogService.ShowInfoAsync("重启成功");
        }
        [RelayCommand]
        private async Task ViewReplicaDetailAsync()
        {
            if (SelectedReplica == null) return;
            ContainerAppReplicaDetailOption option = new ContainerAppReplicaDetailOption
            {
                Replica = SelectedReplica,
                ContainerAppName = _containerApp.Name,
                EnvOption = envOption,
                Management = _management,
            };
            await ShowDialog<ContainerAppReplicaDetailViewModel>(option);
        }

        private async Task LoadDetailAsync()
        {
            IsLoading = true;
            try
            {
                Detail = await _management.GetContainerAppDetailAsync(_containerApp.Name);
                if (Detail?.Revisions != null)
                {
                    Revisions = new ObservableCollection<ContainerAppRevisionInfo>(Detail.Revisions);
                }
                if (Detail?.Replicas != null)
                {
                    Replicas = new ObservableCollection<ContainerAppReplicaInfo>(Detail.Replicas);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 Container App 详情失败：{Name}", _containerApp?.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnDetailChanged(ContainerAppDetail value) => ApplyEnvVarFilter();

        partial void OnEnvVarFilterTextChanged(string value) => ApplyEnvVarFilter();

        private void ApplyEnvVarFilter()
        {
            IEnumerable<ContainerAppEnvVarInfo> source = Detail?.EnvironmentVariables ?? Enumerable.Empty<ContainerAppEnvVarInfo>();
            if (!string.IsNullOrWhiteSpace(EnvVarFilterText))
            {
                string filter = EnvVarFilterText.Trim();
                source = source.Where(x =>
                    (x.Name != null && x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                    (x.Value != null && x.Value.Contains(filter, StringComparison.OrdinalIgnoreCase)));
            }
            FilteredEnvironmentVariables = new ObservableCollection<ContainerAppEnvVarInfo>(source);
        }
    }
}
