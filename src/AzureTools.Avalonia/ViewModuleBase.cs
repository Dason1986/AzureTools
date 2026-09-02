using Microsoft.Extensions.DependencyInjection;

namespace AzureTools
{
    public abstract class ViewModuleBase : IViewModule
    {
        public void Register(AvaloniaComponentsOptions options)
        {
            MapViews(options);
            RegisterServices(options.Services);
        }

        protected abstract void MapViews(AvaloniaComponentsOptions options);

        protected virtual void RegisterServices(IServiceCollection services)
        {
        }
    }
}
