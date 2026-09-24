using AzureTools.SDK;
using AzureTools.SDK.KeyVaults;
using AzureTools.SDK.Models;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.KeyVaults;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;
using Toolkit.Modules.Stores;


namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public partial class KeyVaultSecretMainViewModel : ViewModel
    {
        #region Constructors

        public KeyVaultSecretMainViewModel(IKeyVaultCompareService compareService, IToolkitStore azureToolsStore, IAzureResourceFactory azureResourceFactory, IKeyVaultStore keyVaultOptionStore, IDialogService dialogService, ILogger<KeyVaultSecretMainViewModel> logger)
        {
            _compareService = compareService;
            this.azureToolsStore = azureToolsStore;
            this.azureResourceFactory = azureResourceFactory;
            _dialogService = dialogService;
            _logger = logger;
            this.keyVaultOptionStore = keyVaultOptionStore;
            ProjectList = azureToolsStore.GetProjects().ToList();
            SelectedProject = ProjectList.FirstOrDefault();
            InitializeOptions();
        }

        #endregion

        #region Fields & Properties

        private readonly IKeyVaultCompareService _compareService;
        private readonly IToolkitStore azureToolsStore;
        private readonly IAzureResourceFactory azureResourceFactory;
        private readonly IDialogService _dialogService;
        private readonly ILogger<KeyVaultSecretMainViewModel> _logger;
        private readonly IKeyVaultStore keyVaultOptionStore;

        [ObservableProperty]
        private List<ProjectInfo> _projectList;

        [ObservableProperty]
        private ProjectInfo _selectedProject;

        [ObservableProperty]
        private List<KeyVaultEnv> _keyVaultOptionsList;

        [ObservableProperty]
        private KeyVaultEnv _selectedKeyVaultOption;

        [ObservableProperty]
        private List<KeyVaultEnv> _selectedCompareOptions = new();

        [ObservableProperty]
        private bool _isCompareEnabled;

        #endregion

        public void InitializeOptions()
        {
            ProjectList = azureToolsStore.GetProjects().ToList();
            KeyVaultOptionsList = keyVaultOptionStore.GetOptions();
            if (KeyVaultOptionsList == null || KeyVaultOptionsList.Count == 0)
            {
                _ = _dialogService.ShowWarningAsync(MessageResources.NoEnvConfigured);
                return;
            }

            KeyVaultEnv defaultOption = KeyVaultOptionsList.FirstOrDefault();
            SelectedKeyVaultOption = defaultOption;
            IsCompareEnabled = KeyVaultOptionsList.Count >= 2;
        }

        [RelayCommand]
        private async Task EnterKeyVaultAsync()
        {
            if (SelectedProject == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectValidProject);
                return;
            }

            if (SelectedKeyVaultOption == null)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectValidEnv);
                return;
            }

            if (string.IsNullOrEmpty(SelectedKeyVaultOption.KeyVault))
            {
                await _dialogService.ShowInfoAsync(MessageResources.InvalidEnvConfig);
                return;
            }

            KeyVaultEnv option = SelectedKeyVaultOption;
            IKeyVaultManagement keyVaultManagement = GetKeyVaultManagement(option);
            await ShowDialog<KeyVaultSecretViewModel>(option, SelectedProject, keyVaultManagement);
        }

        [RelayCommand]
        private async Task CompareAsync()
        {
            if (SelectedCompareOptions == null || SelectedCompareOptions.Count < 2)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectTwoEnvs);
                return;
            }

            try
            {
                KeyVaultEnv[] options = SelectedCompareOptions.ToArray();
                KeyVaultAzureEnv[] sdkOptions = options.Select(o => o.ToSdk()).ToArray();
                List<SecretCompareReport> results = await _compareService.CompareAsync(sdkOptions);
                List<string> envNames = options.Select(o => o.Name).ToList();
                KeyVaultSecretCompareOption compareOption = new KeyVaultSecretCompareOption { EnvNames = envNames, Results = results };
                await ShowDialog<KeyVaultSecretCompareViewModel>(compareOption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "对比失败，环境：{Envs}", string.Join(", ", SelectedCompareOptions.Select(o => o.Name)));
                await _dialogService.ShowErrorAsync(string.Format(MessageResources.CompareFailed, ex.Message));
            }
        }

        [RelayCommand]
        private async Task SecretKeyManageAsync()
        {
            await ShowDialog<SecretKeyManageViewModel>(SelectedProject);
        }

        [RelayCommand]
        private async Task EnvManageAsync()
        {
            await ShowDialog<KeyVaultEnvManageViewModel>();
            InitializeOptions();
        }

        public IKeyVaultManagement GetKeyVaultManagement(KeyVaultEnv option)
        {
            AzureTenant sdkTenant = azureToolsStore.GetTenant(option.TenantId).ToSdk();
            KeyVaultAzureEnv sdkEnv = option.ToSdk();
            return azureResourceFactory.GetKeyVaultManagement(sdkEnv, sdkTenant);
        }
    }
}
