using Avalonia.Controls;
using AzureTools.Services;
using AzureTools.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Views.ContainerRuntime;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class BrowserViewModel : ViewModel, IDisposable, IContainerRuntimeDialogHost
    {
        private readonly IContainerRuntimeManager _runtime;
        private readonly SshHostProfile _host;
        private readonly IReadOnlyList<ContainerRuntimeTabViewModel> _tabs;
        private bool _disposed;

        public BrowserViewModel(SshHostProfile host, IContainerRuntimeManagerFactory runtimeFactory, IDialogService dialog, string engine = SshContainerEngine.Docker)
        {
            _host = host;
            WindowTitle = string.Format(SshContainerEngine.GetBrowserTitleFormat(engine), host.Name);
            _runtime = runtimeFactory.Create(host, engine);
            List<ContainerRuntimeTabViewModel> tabs = new List<ContainerRuntimeTabViewModel>
            {
                new ContainersTabViewModel(_runtime, dialog, engine, this),
                new ImagesTabViewModel(_runtime, dialog, engine, this),
                new VolumesTabViewModel(_runtime, dialog, engine),
                new NetworksTabViewModel(_runtime, dialog, engine, this)
            };
            if (_runtime is IPodManager) tabs.Add(new PodsTabViewModel(_runtime, dialog, engine, this));
            _tabs = tabs;
            TabItems = ContainerRuntimeTabViewFactory.Create(engine, _tabs);
        }

        public IReadOnlyList<TabItem> TabItems { get; }

        [ObservableProperty]
        private int _tabIndex;

        partial void OnTabIndexChanged(int value)
        {
            if (value < 0 || value >= _tabs.Count) return;
            _ = _tabs[value].RefreshAsync();
        }

        public Task ShowContainerDetailAsync(ContainerDetailOption option)
        {
            return ShowDialog<ContainerDetailViewModel>(_host, option);
        }

        public Task ShowContainerMonitorAsync(ContainerMonitorOption option)
        {
            return ShowDialog<ContainerMonitorViewModel>(_host, option);
        }

        public Task ShowContainerCreateAsync(ContainerCreateOption option)
        {
            return ShowDialog<ContainerCreateViewModel>(_host, option);
        }

        public Task ShowImageDetailAsync(ImageDetailOption option)
        {
            return ShowDialog<ImageDetailViewModel>(_host, option);
        }

        public Task ShowImageTagsAsync(ImageTagsOption option)
        {
            return ShowDialog<ImageTagsViewModel>(_host, option);
        }

        public Task ShowImageSearchAsync(ImageSearchOption option)
        {
            return ShowDialog<ImageSearchViewModel>(_host, option);
        }

        public Task ShowNetworkDetailAsync(NetworkDetailOption option)
        {
            return ShowDialog<NetworkDetailViewModel>(_host, option);
        }

        public Task ShowPodCreateAsync(PodCreateOption option)
        {
            return ShowDialog<PodCreateViewModel>(_host, option);
        }

        public Task ShowPodEditAsync(PodEditOption option)
        {
            return ShowDialog<PodEditViewModel>(_host, option);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            (_runtime as IDisposable)?.Dispose();
        }
    }
}
