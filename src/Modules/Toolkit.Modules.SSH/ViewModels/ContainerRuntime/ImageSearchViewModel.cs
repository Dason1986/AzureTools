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
    public partial class ImageSearchViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _runtime;
        private readonly IDialogService _dialog;
        private bool _disposed;

        public ImageSearchViewModel(SshHostProfile host, ImageSearchOption option, IContainerRuntimeManagerFactory runtimeFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _runtime = runtimeFactory.Create(host, option.Engine);
            WindowTitle = SSHResource.TitleImageSearch;
        }

        public ObservableCollection<RegistryImage> Images { get; } = new();

        [ObservableProperty]
        private string _keyword;

        [ObservableProperty]
        private RegistryImage _selectedImage;

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        private async Task SearchAsync()
        {
            string keyword = Keyword?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                await _dialog.ShowWarningAsync(SSHResource.MsgImageSearchRequired);
                return;
            }
            IsBusy = true;
            try
            {
                IReadOnlyList<RegistryImage> results = await _runtime.SearchImagesAsync(keyword);
                Images.Clear();
                foreach (RegistryImage image in results) Images.Add(image);
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task PullImageAsync()
        {
            if (SelectedImage == null)
            {
                await _dialog.ShowWarningAsync(SSHResource.MsgImageRequired);
                return;
            }
            await PullAsync(SelectedImage.Name);
        }

        [RelayCommand]
        private async Task PullDirectlyAsync()
        {
            string image = Keyword?.Trim();
            if (string.IsNullOrEmpty(image))
            {
                await _dialog.ShowWarningAsync(SSHResource.MsgImageDirectRequired);
                return;
            }
            await PullAsync(image);
        }

        private async Task PullAsync(string image)
        {
            IsBusy = true;
            try
            {
                await _runtime.PullImageAsync(image);
                await _dialog.ShowInfoAsync(SSHResource.MsgImagePullSuccess);
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_runtime as IDisposable)?.Dispose();
        }
    }
}
