using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ImageTagsViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _docker;
        private readonly IDialogService _dialog;
        private readonly string _imageId;
        private readonly string _engine;
        private bool _disposed;

        public ImageTagsViewModel(SshHostProfile host, ImageTagsOption option, IContainerRuntimeManagerFactory dockerFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _imageId = option.ImageId;
            _engine = option.Engine;
            _docker = dockerFactory.Create(host, option.Engine);
            WindowTitle = string.Format(SSHResource.TitleImageTags, option.RepoTag);
            _ = RefreshAsync();
        }

        public ObservableCollection<string> Tags { get; } = new();

        [ObservableProperty]
        private string _selectedTag;

        [ObservableProperty]
        private string _newTag;

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsBusy = true;
            try
            {
                ImageInspect inspect = await _docker.InspectImageAsync(_imageId);
                Tags.Clear();
                foreach (string tag in inspect?.RepoTags ?? new List<string>())
                {
                    if (string.IsNullOrEmpty(tag) || tag.StartsWith("<none>", StringComparison.Ordinal)) continue;
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(_engine, ex.Message));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddTagAsync()
        {
            string repositoryTag = NewTag?.Trim();
            if (string.IsNullOrEmpty(repositoryTag))
            {
                await _dialog.ShowWarningAsync(SSHResource.MsgImageTagRequired);
                return;
            }
            try
            {
                await _docker.TagImageAsync(_imageId, repositoryTag);
                NewTag = string.Empty;
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task RemoveTagAsync()
        {
            if (string.IsNullOrEmpty(SelectedTag))
            {
                await _dialog.ShowWarningAsync(SSHResource.SelectItemToDelete);
                return;
            }
            if (!await _dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveImageTag, SelectedTag))) return;
            try
            {
                await _docker.RemoveImageAsync(SelectedTag);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_docker as IDisposable)?.Dispose();
        }
    }
}
