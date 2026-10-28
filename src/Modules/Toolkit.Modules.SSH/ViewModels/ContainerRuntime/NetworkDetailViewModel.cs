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
    public partial class NetworkDetailViewModel : ViewModel, IDisposable
    {
        private readonly IContainerRuntimeManager _docker;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private bool _disposed;

        public NetworkDetailViewModel(SshHostProfile host, NetworkDetailOption option, IContainerRuntimeManagerFactory dockerFactory, IDialogService dialog)
        {
            _dialog = dialog;
            _engine = option.Engine;
            _docker = dockerFactory.Create(host, option.Engine);
            NetworkId = option.NetworkId;
            WindowTitle = string.Format(SSHResource.TitleNetworkDetail, option.NetworkName);
            _ = RefreshAsync();
        }

        public string NetworkId { get; }

        public ObservableCollection<NetworkContainer> Containers { get; } = new();

        [ObservableProperty]
        private NetworkContainer _selectedContainer;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                IReadOnlyList<NetworkContainer> items = await _docker.GetNetworkContainersAsync(NetworkId);
                Containers.Clear();
                foreach (NetworkContainer c in items) Containers.Add(c);
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(SshContainerEngine.GetLoadFailedMessage(_engine, ex.Message));
            }
        }

        [RelayCommand]
        private async Task JoinAsync()
        {
            try
            {
                List<Container> running = new List<Container>();
                foreach (Container c in await _docker.ListContainersAsync())
                {
                    if (string.Equals(c.State, "running", StringComparison.OrdinalIgnoreCase)) running.Add(c);
                }
                Dictionary<string, string> mappings = new Dictionary<string, string>
                {
                    [nameof(Container.Name)] = SSHResource.ColContainerName,
                    [nameof(Container.Image)] = SSHResource.ColContainerImage
                };
                IList<Container> picked = await _dialog.SelectItemsAsync(running, false, SSHResource.TitleSelectContainer, mappings);
                if (picked == null || picked.Count == 0) return;
                await _docker.NetworkConnectAsync(NetworkId, picked[0].Id);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                await _dialog.ShowErrorAsync(ex.Message);
            }
        }

        [RelayCommand]
        private async Task LeaveAsync()
        {
            if (SelectedContainer == null) return;
            if (!await _dialog.ShowConfirmAsync(string.Format(SSHResource.MsgConfirmDisconnectNetwork, SelectedContainer.Name))) return;
            try
            {
                await _docker.NetworkDisconnectAsync(NetworkId, SelectedContainer.Id);
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
