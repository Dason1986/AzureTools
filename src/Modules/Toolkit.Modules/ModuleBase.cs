using AzureTools;
using AzureTools.ViewModels;
using AzureTools.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Toolkit.Modules
{
    public abstract class ModuleBase : IModule
    {

        protected ModuleBase(AvaloniaComponentsOptions options)
        {
            this.options = options;
        }
        private readonly AvaloniaComponentsOptions options;
        public void Register()
        {
            RegisterServices(options.Services);
            MapViews();
            RegisterMenus();
        }

        protected abstract void MapViews();

        protected virtual void RegisterMenus()
        {

        }

        protected virtual void RegisterServices(IServiceCollection services)
        {
        }
        protected void MapView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>()
            where TView : View
            where TViewModel : ViewModel
        {
            options.MapView<TView, TViewModel>();
        }

        protected void KeyedMapView<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(
            string key,
            KeyedViewModelFactory viewModelFactory,
            KeyedViewFactory viewFactory)
            where TView : View
            where TViewModel : ViewModel
        {
            options.KeyedMapView<TView, TViewModel>(key, viewModelFactory, viewFactory);
        }

        protected IModuleMenuBuild AddModuleMenu(string code, string name, int order = 0)
        {
            return options.AddModuleMenu(code, name, order);
        }

        protected IModuleMenuBuild GetModuleMenu(string code)
        {
            return options.GetModuleMenu(code);
        }
    }
}
