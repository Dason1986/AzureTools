using AzureTools.Module.Main.ViewModels;
using AzureTools.Module.Main.ViewModels.Options;
using AzureTools.Module.Main.ViewModels.Projects;
using AzureTools.Module.Main.ViewModels.Tenants;
using AzureTools.Module.Main.Views;
using AzureTools.Module.Main.Views.Options;
using AzureTools.Module.Main.Views.Projects;
using AzureTools.Module.Main.Views.Tenants;
using AzureTools.SDK.Abstractions;
using AzureTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTools.Module.Main
{
    public sealed class MainModule : ViewModuleBase
    {
        protected override void MapViews(AvaloniaComponentsOptions options)
        {
            options.MapView<MainView, MainViewModel>();
            options.MapView<LoginView, LoginViewModel>();
            options.MapView<AppWizardView, AppWizardViewModel>();
            options.MapView<OptionsView, OptionsViewModel>();
            options.MapView<ProjectManagementView, ProjectManagementViewModel>();
            options.MapView<ProjectEditorView, ProjectEditorViewModel>();
            options.MapView<TenantManagementView, TenantManagementViewModel>();
            options.MapView<TenantEditorView, TenantEditorViewModel>();
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ITenantProvider, TenantProvider>();
        }
    }
}
