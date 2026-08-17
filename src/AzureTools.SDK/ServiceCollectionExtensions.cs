using Microsoft.Extensions.DependencyInjection;

namespace AzureTools.SDK
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAzure(this IServiceCollection services)
        {
            services.AddSingleton<IAzureResourceFactory, AzureResourceFactory>();
        }
    }
}
