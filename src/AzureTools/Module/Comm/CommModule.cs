using AzureTools.Module.Comm.ViewModels.CliStreamSession;
using AzureTools.Module.Comm.ViewModels.FileBrowser;
using AzureTools.Module.Comm.ViewModels.Tools;
using AzureTools.Module.Comm.Views.CliStreamSession;
using AzureTools.Module.Comm.Views.FileBrowser;
using AzureTools.Module.Comm.Views.Tools;
using AzureTools.Tools.IO;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTools.Module.Comm
{
    public sealed class CommModule : ViewModuleBase
    {
        protected override void MapViews(AvaloniaComponentsOptions options)
        {
            options.MapView<CliStreamSessionView, CliStreamSessionViewModel>();
            options.MapView<FileBrowserView, FileBrowserViewModel>();
            options.MapView<CerFileView, CerFileViewModel>();
            options.MapView<RsaView, RsaViewModel>();
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddKeyedTransient<IFileGenerator, CerFileGenerator>(FileGeneratorKeys.Cer);
            services.AddKeyedTransient<IFileGenerator, RSAPemFileGenerator>(FileGeneratorKeys.Rsa);
        }
    }
}
