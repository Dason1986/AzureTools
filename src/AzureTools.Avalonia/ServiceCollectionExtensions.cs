using AzureTools.Services;
using AzureTools.Services.Platform;
using AzureTools.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;

namespace AzureTools
{

    public static class ServiceCollectionExtensions
    {


        public static void AddAvaloniaComponents(this IServiceCollection services, Action<AvaloniaComponentsOptions> configure = null)
        {
            services.AddSingleton<IDialogService, DefaultDialogService>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSingleton<IMessageFallback, WindowsMessageFallback>();
            }
            services.AddSingleton<IThemeService, DefaultThemeService>();
            services.AddSingleton<IViewFactory, ViewFactory>();
            services.AddScoped<ViewModelContext>();

            AvaloniaComponentsOptions options = new(services);
            configure?.Invoke(options);
            services.AddSingleton<IViewContainerManagement>(options.BuildManagement());
            services.AddSingleton<IMenuManagement>(options.BuildMenus());
        }


    }
}