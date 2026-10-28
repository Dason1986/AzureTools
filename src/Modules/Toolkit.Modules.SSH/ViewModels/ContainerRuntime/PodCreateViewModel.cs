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
    public partial class PodCreateViewModel : ViewModel, IDisposable
    {
        private readonly SshHostProfile _host;
        private readonly IContainerRuntimeManager _runtime;
        private readonly IPodManager _pods;
        private readonly IDialogService _dialog;
        private readonly string _engine;
        private bool _disposed;

        public PodCreateViewModel(SshHostProfile host, PodCreateOption option, IContainerRuntimeManagerFactory runtimeFactory, IDialogService dialog)
        {
            _host = host;
            _dialog = dialog;
            _engine = option.Engine;
            _runtime = runtimeFactory.Create(host, option.Engine);
            _pods = (IPodManager)_runtime;
            WindowTitle = SSHResource.TitleCreatePod;
        }

        public ObservableCollection<ContainerCreateConfig> PendingContainers { get; } = new();

        [ObservableProperty]
        private bool _isYamlMode = true;

        [ObservableProperty]
        private string _yamlText;

        [ObservableProperty]
        private string _podName;

        [ObservableProperty]
        private ContainerCreateConfig _selectedPendingContainer;

        partial void OnSelectedPendingContainerChanged(ContainerCreateConfig value)
        {
            EditPendingContainerCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        private bool _isBusy;

        [RelayCommand]
        private async Task DeployYamlAsync()
        {
            if (string.IsNullOrWhiteSpace(YamlText))
            {
                await _dialog.ShowErrorAsync(SSHResource.MsgYamlRequired);
                return;
            }
            await ExecuteAsync(SSHResource.MsgPodDeployed, () => _pods.PlayKubeYamlAsync(YamlText));
        }

        [RelayCommand]
        private async Task AddContainerAsync()
        {
            if (string.IsNullOrWhiteSpace(PodName))
            {
                await _dialog.ShowErrorAsync(SSHResource.MsgPodNameRequired);
                return;
            }
            ContainerCreateViewModel dialog = await ShowDialog<ContainerCreateViewModel>(_host, new ContainerCreateOption
            {
                Engine = _engine,
                PodName = PodName.Trim(),
                CollectOnly = true
            });
            if (dialog?.CollectedConfig != null) PendingContainers.Add(dialog.CollectedConfig);
        }

        [RelayCommand]
        private void RemovePendingContainer()
        {
            if (SelectedPendingContainer != null) PendingContainers.Remove(SelectedPendingContainer);
        }

        private bool HasSelectedPendingContainer()
        {
            return SelectedPendingContainer != null;
        }

        [RelayCommand(CanExecute = nameof(HasSelectedPendingContainer))]
        private async Task EditPendingContainerAsync()
        {
            ContainerCreateConfig original = SelectedPendingContainer;
            ContainerCreateViewModel dialog = await ShowDialog<ContainerCreateViewModel>(_host, new ContainerCreateOption
            {
                Engine = _engine,
                PodName = PodName?.Trim(),
                CollectOnly = true,
                InitialConfig = original
            });
            if (dialog?.CollectedConfig == null) return;
            int index = PendingContainers.IndexOf(original);
            if (index >= 0) PendingContainers[index] = dialog.CollectedConfig;
        }

        [RelayCommand]
        private async Task CreatePodAsync()
        {
            if (string.IsNullOrWhiteSpace(PodName))
            {
                await _dialog.ShowErrorAsync(SSHResource.MsgPodNameRequired);
                return;
            }
            if (PendingContainers.Count == 0)
            {
                await _dialog.ShowErrorAsync(SSHResource.MsgContainerRequired);
                return;
            }
            string name = PodName.Trim();
            await ExecuteAsync(string.Format(SSHResource.MsgPodCreated, name), async () =>
            {
                await _pods.CreatePodAsync(name);
                foreach (ContainerCreateConfig config in PendingContainers)
                {
                    await _pods.CreateContainerInPodAsync(name, config);
                }
            });
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await CloseAsync();
        }

        private async Task ExecuteAsync(string successMessage, Func<Task> action)
        {
            IsBusy = true;
            try
            {
                await action();
                await _dialog.ShowInfoAsync(successMessage);
                await CloseAsync();
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_runtime as IDisposable)?.Dispose();
        }
    }
}
