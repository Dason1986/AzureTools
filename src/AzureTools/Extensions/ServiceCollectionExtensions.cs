using AzureTools.SDK;
using AzureTools.Stores;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Toolkit.Modules.Azure;
using Toolkit.Modules.Comm;
using Toolkit.Modules.Main;

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
        }
        public static void AddComponents(this IServiceCollection services)
        {
            services.AddAzure();
            services.AddAvaloniaComponents(opt =>
            {
                new MainModule(opt).Register();
                new AzureModule(opt).Register();
                new CommModule(opt).Register();
            });
        }
    }
}