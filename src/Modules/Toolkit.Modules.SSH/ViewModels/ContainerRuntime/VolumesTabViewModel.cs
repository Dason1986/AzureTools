using AzureTools.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class VolumesTabViewModel : ContainerRuntimeTabViewModel
    {
        public VolumesTabViewModel(IContainerRuntimeManager runtime, IDialogService dialog, string engine)
            : base(runtime, dialog, engine)
        {
        }

        public ObservableCollection<Volume> Volumes { get; } = new();

        [ObservableProperty]
        private Volume _selectedVolume;

        protected override async Task LoadAsync()
        {
            Volumes.Clear();
            foreach (Volume volume in await Runtime.ListVolumesAsync()) Volumes.Add(volume);
        }

        [RelayCommand]
        private async Task CreateVolumeAsync()
        {
            string name = await Dialog.ShowPromptDialogAsync(SSHResource.TitleCreateVolume, SSHResource.PromptVolumeName);
            if (string.IsNullOrWhiteSpace(name)) return;
            try { await Runtime.CreateVolumeAsync(name.Trim()); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task RemoveVolumeAsync()
        {
            if (SelectedVolume == null) return;
            if (!await Dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveVolume, SelectedVolume.Name))) return;
            try { await Runtime.RemoveVolumeAsync(SelectedVolume.Name); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }

        [RelayCommand]
        private async Task PruneVolumesAsync()
        {
            if (!await Dialog.ShowConfirmAsync(SSHResource.MsgConfirmPruneVolumes)) return;
            try { await Runtime.PruneVolumesAsync(); await RefreshAsync(); }
            catch (Exception ex) { await Dialog.ShowErrorAsync(ex.Message); }
        }
    }
}
