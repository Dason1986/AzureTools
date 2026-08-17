using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK.ContainerRegistries;
using AzureTools.Services;
using AzureTools.Views.ContainerRegistries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AzureTools.ViewModels.ContainerRegistries
{
    public partial class ContainerRegistryRepositoryListViewModel : ViewModel
    {
        #region Constructors

        public ContainerRegistryRepositoryListViewModel(ContainerRegistryEnv option, RepositoryInfo[] repositories, IContainerRegistryManagement management, IDialogService dialogService, ILogger<ContainerRegistryRepositoryListViewModel> logger)
        {
            _option = option;
            _management = management;
            _dialogService = dialogService;
            _logger = logger;
            Repositories = repositories == null || repositories.Length == 0
                ? new ObservableCollection<RepositoryInfo>()
                : new ObservableCollection<RepositoryInfo>(repositories.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase));
            SelectedRepository = Repositories.FirstOrDefault();
        }

        #endregion

        #region Fields & Properties

        private readonly ContainerRegistryEnv _option;
        private readonly IContainerRegistryManagement _management;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ContainerRegistryRepositoryListViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<RepositoryInfo> _repositories;

        [ObservableProperty]
        private RepositoryInfo _selectedRepository;

        [ObservableProperty]
        private ObservableCollection<ManifestProperty> _manifests;

        [ObservableProperty]
        private IList<ManifestProperty> _selectedManifests = new ObservableCollection<ManifestProperty>();

        [ObservableProperty]
        private bool _isDeleting;

        [ObservableProperty]
        private bool _isLoading;

        public bool SupportManifestDetail => _management.SupportManifestDetail;

        public bool SupportRename => _management.SupportRename;

        public bool SupportFixedManifest => _management.SupportFixedManifest;

        public string WindowTitle => string.Format(FormResources.TitleContainerRegistryWindow, _option?.Name);

        #endregion

        partial void OnSelectedRepositoryChanged(RepositoryInfo value)
        {
            Manifests = value?.ManifestProperties == null
                ? new ObservableCollection<ManifestProperty>()
                : new ObservableCollection<ManifestProperty>(value.ManifestProperties);
            SelectedManifests.Clear();
        }

        [RelayCommand]
        private async Task DeleteManifestsAsync()
        {
            if (SelectedManifests == null || SelectedManifests.Count == 0)
            {
                await _dialogService.ShowInfoAsync(MessageResources.SelectItemToDelete);
                return;
            }

            ManifestProperty[] snapshots = SelectedManifests.ToArray();
            ManifestProperty[] deletableItems = snapshots.Where(m => m.CanDelete).ToArray();
            int fixedCount = snapshots.Length - deletableItems.Length;

            if (deletableItems.Length == 0)
            {
                await _dialogService.ShowInfoAsync(MessageResources.NoManifestsToDelete);
                return;
            }

            string confirmMessage = fixedCount > 0
                ? string.Format(MessageResources.ConfirmDeleteManifestsWithFixed, deletableItems.Length, fixedCount)
                : string.Format(MessageResources.ConfirmDeleteManifests, deletableItems.Length);

            bool confirmed = await _dialogService.ShowConfirmAsync(confirmMessage);
            if (!confirmed) return;

            IsDeleting = true;
            try
            {
                int failures = 0;
                foreach (ManifestProperty manifest in deletableItems)
                {
                    try
                    {
                        await _management.DeleteManifest(manifest.RepositoryName, manifest.Digest);
                        Manifests.Remove(manifest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "删除 Manifest 失败：{Repository}/{Digest}", manifest.RepositoryName, manifest.Digest);
                        failures++;
                    }
                }

                if (SelectedRepository != null)
                {
                    SelectedRepository.ManifestProperties = Manifests.ToArray();
                }

                SelectedManifests.Clear();

                if (failures > 0)
                {
                    await _dialogService.ShowErrorAsync(string.Format(MessageResources.DeleteManifestFailed, failures, deletableItems.Length));
                }
            }
            finally
            {
                IsDeleting = false;
            }
        }

        [RelayCommand]
        private async Task ViewManifestDetail(ManifestProperty manifest)
        {
            if (manifest == null) return;
            await ShowDialog<ContainerRegistryManifestDetailView, ContainerRegistryManifestDetailViewModel>(manifest, _management);
        }

        [RelayCommand]
        private async Task RenameTagAsync(ManifestProperty manifest)
        {
            if (manifest == null) return;

            if (manifest.Tags == null || manifest.Tags.Length == 0)
            {
                await _dialogService.ShowInfoAsync(MessageResources.NoTagsToRename);
                return;
            }

            string oldTagName = await SelectOldTagAsync(manifest.Tags);
            if (oldTagName == null) return;

            string newTagName = await _dialogService.ShowPromptDialogAsync(
                FormResources.TitleRenameTag,
                FormResources.PromptNewTagName,
                oldTagName);

            if (newTagName == null) return;
            if (string.IsNullOrWhiteSpace(newTagName))
            {
                await _dialogService.ShowWarningAsync(MessageResources.TagCannotBeEmpty);
                return;
            }
            newTagName = newTagName.Trim();
            if (string.Equals(oldTagName, newTagName, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                await _management.RenameTag(manifest.RepositoryName, manifest.Digest, oldTagName, newTagName);
                UpdateManifestTags(manifest, oldTagName, newTagName);
                await _dialogService.ShowInfoAsync(MessageResources.RenameTagSuccess);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重命名 Tag 失败：{Repository}/{Digest}/{Tag}", manifest.RepositoryName, manifest.Digest, oldTagName);
                await _dialogService.ShowErrorAsync(ex.Message);
            }
        }

        // 选择要重命名的旧 Tag：单个时直接使用，多个时让用户输入
        private async Task<string> SelectOldTagAsync(string[] tags)
        {
            if (tags.Length == 1) return tags[0];

            string tagsList = string.Join(", ", tags);
            string prompt = $"{MessageResources.SelectTagToRename}（{tagsList}）";
            string input = await _dialogService.ShowPromptDialogAsync(FormResources.TitleRenameTag, prompt);
            if (input == null) return null;
            input = input.Trim();
            return Array.Exists(tags, t => string.Equals(t, input, StringComparison.Ordinal)) ? input : null;
        }

        // 更新本地 manifest 的 Tags 数组并刷新 UI
        private void UpdateManifestTags(ManifestProperty manifest, string oldTagName, string newTagName)
        {
            string[] newTags = manifest.Tags.Select(t => string.Equals(t, oldTagName, StringComparison.Ordinal) ? newTagName : t).ToArray();
            manifest.Tags = newTags;

            int index = Manifests.IndexOf(manifest);
            if (index >= 0)
            {
                Manifests[index] = manifest;
            }

            if (SelectedRepository != null)
            {
                SelectedRepository.ManifestProperties = Manifests.ToArray();
            }
        }

        [RelayCommand]
        private async Task FixedManifestAsync(ManifestProperty manifest)
        {
            if (manifest == null) return;

            bool isFixed = !manifest.CanDelete;
            bool confirmed = await _dialogService.ShowConfirmAsync(
                isFixed ? MessageResources.ConfirmUnfixedManifest : MessageResources.ConfirmFixedManifest);
            if (!confirmed) return;

            try
            {
                bool shouldFix = !isFixed;
                await _management.FixedManifest(manifest.RepositoryName, manifest.Digest, shouldFix);
                manifest.CanDelete = isFixed;

                int index = Manifests.IndexOf(manifest);
                if (index >= 0)
                {
                    Manifests[index] = manifest;
                }

                if (SelectedRepository != null)
                {
                    SelectedRepository.ManifestProperties = Manifests.ToArray();
                }

                await _dialogService.ShowInfoAsync(
                    shouldFix ? MessageResources.FixedManifestSuccess : MessageResources.UnfixedManifestSuccess);
            }
            catch (Exception ex)
            {
                string operation = isFixed ? "取消固定" : "固定";
                _logger.LogError(ex, "{Operation} Manifest 失败：{Repository}/{Digest}", operation, manifest.RepositoryName, manifest.Digest);
                await _dialogService.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsLoading = true;
            try
            {
                RepositoryInfo[] repositories = await _management.GetAllRepositories();
                Repositories = repositories == null || repositories.Length == 0
                    ? new ObservableCollection<RepositoryInfo>()
                    : new ObservableCollection<RepositoryInfo>(repositories.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase));
                SelectedRepository = Repositories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新仓库列表失败");
                await _dialogService.ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
