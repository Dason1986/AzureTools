using AzureTools.Properties;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureTools.Module.Azure.ViewModels.ContainerRegistries
{
    public partial class ContainerRegistryManifestDetailViewModel : ViewModel
    {
        #region Constructors

        public ContainerRegistryManifestDetailViewModel(ManifestProperty manifest, IContainerRegistryManagement management, IDialogService dialogService, ILogger<ContainerRegistryManifestDetailViewModel> logger)
        {
            _manifest = manifest;
            _management = management;
            _dialogService = dialogService;
            _logger = logger;
            _ = LoadDetailAsync();
        }

        #endregion

        #region Fields & Properties

        private readonly ManifestProperty _manifest;
        private readonly IContainerRegistryManagement _management;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ContainerRegistryManifestDetailViewModel> _logger;

        [ObservableProperty]
        private ManifestDetail _detail;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _windowTitle;

        #endregion

        private async Task LoadDetailAsync()
        {
            IsLoading = true;
            try
            {
                Detail = await _management.GetManifestDetailAsync(_manifest.RepositoryName, _manifest.Digest);
                WindowTitle = string.Format(FormResources.TitleManifestDetail, _manifest.RepositoryName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 Manifest 详情失败：{Repository}/{Digest}", _manifest.RepositoryName, _manifest.Digest);
                await _dialogService.ShowErrorAsync(ex.InnerException?.Message ?? ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
