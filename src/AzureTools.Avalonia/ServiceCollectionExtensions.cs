using AzureTools.Services;
using AzureTools.Services.Platform;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

namespace AzureTools
{

    public static class ServiceCollectionExtensions
    {


        public static void AddAvaloniaComponents(this IServiceCollection services)
        {

            services.AddSingleton<IDialogService, DefaultDialogService>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                services.AddSingleton<IMessageFallback, WindowsMessageFallback>();
            }
            services.AddSingleton<IThemeService, DefaultThemeService>();



            services.AddSingleton<IViewFactory, ViewFactory>();
        }


    }
}