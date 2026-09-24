using AzureTools;
using AzureTools.Cli;
using AzureTools.Services;
using AzureTools.Tools.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolkit.Modules.Comm.ViewModels.CliStreamSession;
using Toolkit.Modules.Comm.ViewModels.FileBrowser;
using Toolkit.Modules.Comm.ViewModels.Tools;
using Toolkit.Modules.Comm.Views.CliStreamSession;
using Toolkit.Modules.Comm.Views.FileBrowser;
using Toolkit.Modules.Comm.Views.Tools;

namespace Toolkit.Modules.Comm
{
    public sealed class CommModule : ModuleBase
    {
        public CommModule(AvaloniaComponentsOptions options) : base(options)
        {
        }

        protected override void MapViews()
        {
            KeyedMapView<CliStreamSessionView, CliStreamSessionViewModel>(
                CommViewKeys.CliStreamSession,
                (p, sp) => new CliStreamSessionViewModel(
                    p.Get<ICliStreamSession>(CommViewKeys.CliStreamSessionParams.Session),
                    p.Get<string>(CommViewKeys.CliStreamSessionParams.Title),
                    p.Get<bool>(CommViewKeys.CliStreamSessionParams.IsInteractive)),
                static (vm, ctx) => new CliStreamSessionView((CliStreamSessionViewModel)vm, ctx));

            KeyedMapView<FileBrowserView, FileBrowserViewModel>(
                CommViewKeys.FileBrowser,
                (p, sp) => new FileBrowserViewModel(
                    p.Get<IContainerFileProvider>(CommViewKeys.FileBrowserParams.Provider),
                    p.Get<string>(CommViewKeys.FileBrowserParams.Title),
                    sp.GetRequiredService<IDialogService>()),
                static (vm, ctx) => new FileBrowserView((FileBrowserViewModel)vm, ctx));

            KeyedMapView<CerFileView, CerFileViewModel>(
                CommViewKeys.Cer,
                static (p, sp) => new CerFileViewModel(
                    sp.GetRequiredKeyedService<IFileGenerator>(FileGeneratorKeys.Cer),
                    sp.GetRequiredService<IDialogService>(),
                    sp.GetRequiredService<ILogger<CerFileViewModel>>()),
                static (vm, ctx) => new CerFileView((CerFileViewModel)vm, ctx));

            KeyedMapView<RsaView, RsaViewModel>(
                CommViewKeys.Rsa,
                static (p, sp) => new RsaViewModel(
                    sp.GetRequiredKeyedService<IFileGenerator>(FileGeneratorKeys.Rsa),
                    sp.GetRequiredService<IDialogService>(),
                    sp.GetRequiredService<ILogger<RsaViewModel>>()),
                static (vm, ctx) => new RsaView((RsaViewModel)vm, ctx));
        }



        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddKeyedTransient<IFileGenerator, CerFileGenerator>(FileGeneratorKeys.Cer);
            services.AddKeyedTransient<IFileGenerator, RSAPemFileGenerator>(FileGeneratorKeys.Rsa);
        }
    }
}
