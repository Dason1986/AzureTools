using AzureTools;
using Microsoft.Extensions.DependencyInjection;
using System;
using Toolkit.Modules.SSH.ContainerRuntime;
using Toolkit.Modules.SSH.Managers;
using Toolkit.Modules.SSH.Stores;
using Toolkit.Modules.SSH.ViewModels.ContainerRuntime;
using Toolkit.Modules.SSH.ViewModels.FirewallManagement;
using Toolkit.Modules.SSH.ViewModels.PackageManagement;
using Toolkit.Modules.SSH.ViewModels.SshHosts;
using Toolkit.Modules.SSH.Views.ContainerRuntime;
using Toolkit.Modules.SSH.Views.FirewallManagement;
using Toolkit.Modules.SSH.Views.PackageManagement;
using Toolkit.Modules.SSH.Views.SshHosts;

namespace Toolkit.Modules.SSH
{
    public sealed class SSHModule : ModuleBase
    {
        public SSHModule(AvaloniaComponentsOptions options) : base(options)
        {
        }

        protected override void MapViews()
        {
            MapView<SshHostManagementView, SshHostManagementViewModel>();
            MapView<SshHostEditorView, SshHostEditorViewModel>();
            MapView<NetworkDetailView, NetworkDetailViewModel>();
            MapView<ContainerDetailView, ContainerDetailViewModel>();
            MapView<ContainerMonitorView, ContainerMonitorViewModel>();
            MapView<ImageDetailView, ImageDetailViewModel>();
            MapView<ImageTagsView, ImageTagsViewModel>();
            MapView<ImageSearchView, ImageSearchViewModel>();
            MapView<ContainerCreateView, ContainerCreateViewModel>();
            MapView<PodCreateView, PodCreateViewModel>();
            MapView<PodEditView, PodEditViewModel>();
            KeyedMapView<BrowserView, BrowserViewModel>(
                SshViewKeys.DockerBrowser,
                (p, sp) => new BrowserViewModel(
                    p.Get<Models.SshHostProfile>(SshViewKeys.DockerBrowserParams.Host),
                    sp.GetRequiredService<IContainerRuntimeManagerFactory>(),
                    sp.GetRequiredService<AzureTools.Services.IDialogService>(),
                    Models.SshContainerEngine.Docker),
                static (vm, ctx) => new BrowserView((BrowserViewModel)vm, ctx));
            KeyedMapView<BrowserView, BrowserViewModel>(
                SshViewKeys.PodmanBrowser,
                (p, sp) => new BrowserViewModel(
                    p.Get<Models.SshHostProfile>(SshViewKeys.DockerBrowserParams.Host),
                    sp.GetRequiredService<IContainerRuntimeManagerFactory>(),
                    sp.GetRequiredService<AzureTools.Services.IDialogService>(),
                    Models.SshContainerEngine.Podman),
                static (vm, ctx) => new BrowserView((BrowserViewModel)vm, ctx));
            KeyedMapView<PackageManagerView, PackageManagerViewModel>(
                SshViewKeys.PackageManager,
                (p, sp) => new PackageManagerViewModel(
                    p.Get<Models.SshHostProfile>(SshViewKeys.PackageManagerKeys.Host),
                    sp.GetRequiredService<Managers.ISshConnectionManager>(),
                    sp.GetRequiredService<AzureTools.Services.IDialogService>()),
                static (vm, ctx) => new PackageManagerView((PackageManagerViewModel)vm, ctx));
            KeyedMapView<FirewallManagerView, FirewallManagerViewModel>(
                SshViewKeys.FirewallManager,
                (p, sp) => new FirewallManagerViewModel(
                    p.Get<Models.SshHostProfile>(SshViewKeys.FirewallManagerKeys.Host),
                    sp.GetRequiredService<Managers.ISshConnectionManager>(),
                    sp.GetRequiredService<AzureTools.Services.IDialogService>()),
                static (vm, ctx) => new FirewallManagerView((FirewallManagerViewModel)vm, ctx));
        }

        protected override void RegisterMenus()
        {
            IModuleMenuBuild moduleMenu = AddModuleMenu("SSH", "SSH");
            moduleMenu.AddMenu(code: "SshHostManage", name: "SSH主机管理", icon: Properties.SSHResource.Compute, viewModelType: typeof(SshHostManagementViewModel));
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ISshHostStore, DefaultSshHostStore>();
            services.AddSingleton<ISshConnectionManager, DefaultSshConnectionManager>();
            services.AddSingleton<ISshConnectionTestService, DefaultSshConnectionTestService>();
            services.AddSingleton<ISshTunnelManager, DefaultSshTunnelManager>();
            services.AddSingleton(sp => new Lazy<ISshTunnelManager>(sp.GetRequiredService<ISshTunnelManager>));
            services.AddSingleton<IContainerRuntimeManagerFactory, DefaultContainerRuntimeManagerFactory>();
        }
    }
}
