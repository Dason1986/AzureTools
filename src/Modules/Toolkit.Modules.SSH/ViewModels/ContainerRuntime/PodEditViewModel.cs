using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Properties;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class PodEditViewModel : ViewModel, IDisposable
    {
        private readonly SshHostProfile _host;
        private readonly IContainerRuntimeManager _runtime;
        private readonly IPodManager _pods;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private bool _disposed;

        public PodEditViewModel(SshHostProfile host, PodEditOption option, IContainerRuntimeManagerFactory runtimeFactory, IDialogService dialog)
        {
            _host = host;
            _dialog = dialog;
            _engine = option.Engine;
            _runtime = runtimeFactory.Create(host, option.Engine);
            _pods = (IPodManager)_runtime;
            PodName = option.PodName;
            WindowTitle = string.Format(SSHResource.TitleEditPod, option.PodName);
            _ = LoadContainersAsync();
        }

        public string PodName { get; }

        public ObservableCollection<Container> Containers { get; } = new();

        [ObservableProperty]
        private Container _selectedContainer;

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        private async Task LoadContainersAsync()
        {
            IsBusy = true;
            try
            {
                Containers.Clear();
                foreach (Container container in await _pods.ListPodContainersAsync(PodName))
                {
                    Containers.Add(container);
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
        private async Task AddContainerAsync()
        {
            await ShowDialog<ContainerCreateViewModel>(_host, new ContainerCreateOption { Engine = _engine, PodName = PodName });
            await LoadContainersAsync();
        }

        [RelayCommand]
        private async Task RemoveContainerAsync()
        {
            if (SelectedContainer == null) return;
            if (!await _dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmRemoveContainer, SelectedContainer.Name))) return;
            IsBusy = true;
            try
            {
                await _runtime.RemoveContainerAsync(SelectedContainer.Id, true);
                await LoadContainersAsync();
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
        private async Task DoneAsync()
        {
            await CloseAsync();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_runtime as IDisposable)?.Dispose();
        }
    }
}
