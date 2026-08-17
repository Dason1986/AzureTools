using Avalonia;
using AzureTools.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;

namespace AzureTools
{
    internal static class Program
    {
        private static IHost host;

        [STAThread]
        static void Main(string[] args)
        {
            CreateHoust();

            App.InitializeServices(host.Services);

            AppBuilder builder = AppBuilder.Configure<App>()
                .UsePlatformDetect();
            builder.StartWithClassicDesktopLifetime(args);

        }

        private static void CreateHoust()
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();
            ConfigureServices(builder.Services);
            host = builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "azuretools-.log");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, rollOnFileSizeLimit: false)
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            services.AddJson();
            services.AddStore();
            services.AddComponents();

            services.AddViews();
        }
    }
}