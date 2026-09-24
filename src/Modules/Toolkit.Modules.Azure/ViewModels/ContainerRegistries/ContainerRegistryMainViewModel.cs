using AzureTools.SDK;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;


namespace Toolkit.Modules.Azure.ViewModels.ContainerRegistries
{
    public partial class ContainerRegistryMainViewModel : ViewModel
    {
        #region Constructors

        public ContainerRegistryMainViewModel(IContainerRegistryStore containerRegistryStore, IToolkitStore azureToolsStore, IAzureResourceFactory azureResourceFactory, IDialogService dialogService, ILogger<ContainerRegistryMainViewModel> logger)
        {
            _containerRegistryStore = containerRegistryStore;
            _azureToolsStore = azureToolsStore;
            _azureResourceFactory = azureResourceFactory;
            _dialogService = dialogService;
            _logger = logger;
            InitializeOptions();
        }

        #endregion

        #region Fields & Properties

        private readonly IContainerRegistryStore _containerRegistryStore;
        private readonly IToolkitStore _azureToolsStore;
        private readonly IAzureResourceFactory _azureResourceFactory;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ContainerRegistryMainViewModel> _logger;

        [ObservableProperty]
        private List<ContainerRegistryEnv> _containerRegistryOptionsList;

        [ObservableProperty]
        private ContainerRegistryEnv _selectedContainerRegistryOption;

        #endregion

        public void InitializeOptions()
        {
            ContainerRegistryOptionsList = _containerRegistryStore.GetOptions();
            if (ContainerRegistryOptionsList == null || ContainerRegistryOptionsList.Count == 0)
            {
                _ = _dialogService.ShowWarningAsync(MessageResources.NoEnvConfigured);
                return;
            }

            SelectedContainerRegistryOption = ContainerRegistryOptionsList[0];
        }

        [RelayCommand]
        private async Task QueryAsync()
        {
            if (SelectedContainerRegistryOption == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectValidEnv);
                return;
            }

            if (string.IsNullOrEmpty(SelectedContainerRegistryOption.Registry))
            {
                await _dialogService.ShowInfoAsync(MessageResources.InvalidEnvConfig);
                return;
            }

            ContainerRegistryEnv option = SelectedContainerRegistryOption;
            TenantProfile profile = _azureToolsStore.GetTenant(option.TenantId);
            if (profile == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.NoTenantSelected);
                return;
            }

            try
            {
                IContainerRegistryManagement management = _azureResourceFactory.GetContainerRegistryManagement(option.ToSdk(), profile.ToSdk());
                RepositoryInfo[] repositories = await management.GetAllRepositoriesAsync();
                await OpenRepositoryListAsync(option, repositories, management);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询容器注册表仓库列表失败：{Name}", option.Name);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [RelayCommand]
        private async Task EnvManageAsync()
        {
            await ShowDialog<ContainerRegistryEnvManageViewModel>();
            InitializeOptions();
        }

        private async Task OpenRepositoryListAsync(ContainerRegistryEnv option, RepositoryInfo[] repositories, IContainerRegistryManagement management)
        {
            await ShowDialog<ContainerRegistryRepositoryListViewModel>(option, repositories, management);
        }
    }
}
