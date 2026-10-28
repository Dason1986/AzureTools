using AzureTools.Services;
using AzureTools.Tools.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class ImagesTabViewModel : ContainerRuntimeTabViewModel
    {
        private readonly IContainerRuntimeDialogHost _dialogHost;

        public ImagesTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine, IContainerRuntimeDialogHost dialogHost)
            : base(runtime, dialog, engine)
        {
            _dialogHost = dialogHost;
        }

        public ObservableCollection<Image> Images { get; } = new();

        [ObservableProperty]
        private Image _selectedImage;

        protected override async Task LoadAsync()
        {
            Images.Clear();
            foreach (Image image in await Runtime.ListImagesAsync()) Images.Add(image);
        }

        [RelayCommand]
        private async Task RemoveImageAsync()
        {
            if (SelectedImage == null) return;
            if (!await Dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveImage, SelectedImage.RepoTag))) return;
            try { await Runtime.RemoveImageAsync(SelectedImage.Id, true); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task DetailImageAsync()
        {
            if (SelectedImage == null) return;
            await _dialogHost.ShowImageDetailAsync(new ImageDetailOption
            {
                ImageId = SelectedImage.Id,
                RepoTag = SelectedImage.RepoTag,
                Engine = Engine
            });
        }

        [RelayCommand]
        private async Task PruneImagesAsync()
        {
            if (!await Dialog.ShowConfirmAsync(SSHResource.MsgConfirmPruneImages)) return;
            try { await Runtime.PruneImagesAsync(); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task PullImageAsync()
        {
            await _dialogHost.ShowImageSearchAsync(new ImageSearchOption { Engine = Engine });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task TagImageAsync()
        {
            Image image = SelectedImage;
            if (image == null) return;
            await _dialogHost.ShowImageTagsAsync(new ImageTagsOption { ImageId = image.Id, RepoTag = image.RepoTag, Engine = Engine });
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task ExportImageAsync()
        {
            Image image = SelectedImage;
            if (image == null) return;
            IFileEntry destination = await Dialog.SaveFileAsync(SSHResource.ImageArchiveFileFilter, BuildArchiveFileName(image));
            if (destination == null) return;
            await ExecuteImageOperationAsync(async () =>
            {
                using Stream stream = destination.GetWrite();
                await Runtime.ExportImageAsync(image.Id, stream);
            }, string.Format(SSHResource.MsgImageExportSuccess, destination.FullPath));
        }

        [RelayCommand]
        private async Task ImportImageAsync()
        {
            IFileEntry source = await Dialog.OpenFileAsync(SSHResource.ImageArchiveFileFilter, null);
            if (source == null) return;
            await ExecuteImageOperationAsync(async () =>
            {
                using Stream stream = source.GetRead();
                await Runtime.ImportImageAsync(stream);
            }, SSHResource.MsgImageImportSuccess);
        }

        private async Task ExecuteImageOperationAsync(Func<Task> operation, string successMessage)
        {
            IsBusy = true;
            try
            {
                await operation();
                await RefreshAsync();
                if (!string.IsNullOrEmpty(successMessage)) await Dialog.ShowInfoAsync(successMessage);
            }
            catch (Exception ex)
            {
                await Dialog.ShowErrorAsync(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string BuildArchiveFileName(Image image)
        {
            string name = string.IsNullOrEmpty(image.RepoTag) || image.RepoTag.StartsWith("<none>", StringComparison.Ordinal)
                ? image.Id
                : image.RepoTag;
            if (string.IsNullOrEmpty(name)) name = "image";
            foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
            return $"{name.Replace(':', '_').Replace('/', '_')}.tar";
        }
    }
}
