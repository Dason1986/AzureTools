
using AzureTools;
using Microsoft.Extensions.DependencyInjection;
using Toolkit.Modules.Main.Properties;
using Toolkit.Modules.Main.ViewModels;
using Toolkit.Modules.Main.ViewModels.Options;
using Toolkit.Modules.Main.ViewModels.Projects;
using Toolkit.Modules.Main.Views;
using Toolkit.Modules.Main.Views.Options;
using Toolkit.Modules.Main.Views.Projects;
using Toolkit.Modules.Stores;

namespace Toolkit.Modules.Main
{
    public sealed class MainModule : ModuleBase
    {
        public MainModule(AvaloniaComponentsOptions options) : base(options)
        {
        }

        protected override void MapViews()
        {
            MapView<MainView, MainViewModel>();
            MapView<LoginView, LoginViewModel>();
            MapView<AppWizardView, AppWizardViewModel>();
            MapView<OptionsView, OptionsViewModel>();
            MapView<ProjectManagementView, ProjectManagementViewModel>();
            MapView<ProjectEditorView, ProjectEditorViewModel>();





        }
        protected override void RegisterMenus()
        {
            IModuleMenuBuild moduleMenu = AddModuleMenu("Main", MainResource.ModuleMain, -1);
            moduleMenu.AddMenu(code: "Projects", name: MainResource.MenuProjects, icon: ImageResource.Projects, viewModelType: typeof(ProjectManagementViewModel));
            moduleMenu.AddMenu(code: "Options", name: MainResource.MenuOptions, icon: ImageResource.Options, viewModelType: typeof(OptionsViewModel));

        }
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IToolkitStore, DefaultToolkitStore>();
        }
    }
}
