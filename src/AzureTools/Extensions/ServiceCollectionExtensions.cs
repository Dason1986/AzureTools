using AzureTools.Module.Azure;
using AzureTools.Module.Comm;
using AzureTools.Module.Main;
using AzureTools.SDK;
using AzureTools.Stores;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AzureTools.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJson(this IServiceCollection services)
        {
            services.Configure<JsonSerializerOptions>(options =>
            {
                options.WriteIndented = false;
                options.PropertyNameCaseInsensitive = true;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs);
                options.TypeInfoResolver = AppJsonSerializerContext.Default;
            });
            return services;
        }
        public static void AddStore(this IServiceCollection services)
        {
            services.AddSingleton<IStoreContext, FileStoreContext>();
            services.AddSingleton<IKeyVaultStore, DefaultKeyVaultsStore>();
            services.AddSingleton<IContainerRegistryStore, DefaultContainerRegistryStore>();
            services.AddSingleton<IAppContainerStore, DefaultAppContainerStore>();
            services.AddSingleton<IAzureToolsStore, DefaultAzureToolsStore>();
        }
        public static void AddComponents(this IServiceCollection services)
        {
            services.AddAzure();
            services.AddAvaloniaComponents(opt =>
            {
                new MainModule().Register(opt);
                new AzureModule().Register(opt);
                new CommModule().Register(opt);
            });
        }
    }
}