using AzureTools.SDK;
using AzureTools.SDK.Abstractions;
using AzureTools.Services;
using AzureTools.Services.KeyVaults;
using AzureTools.Stores;
using AzureTools.Tools.IO;
using AzureTools.ViewModels;
using AzureTools.ViewModels.AppContainers;
using AzureTools.ViewModels.ContainerRegistries;
using AzureTools.ViewModels.Options;
using AzureTools.ViewModels.Projects;
using AzureTools.ViewModels.Tenants;
using AzureTools.Views;
using AzureTools.Views.AppContainers;
using AzureTools.Views.ContainerRegistries;
using AzureTools.Views.KeyVaults;
using AzureTools.Views.Options;
using AzureTools.Views.Projects;
using AzureTools.Views.Tenants;
using AzureTools.Views.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
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
            services.AddAvaloniaComponents();
            services.AddScoped<ITenantProvider, TenantProvider>();


            services.AddKeyedTransient<IFileGenerator, CerFileGenerator>(FileGeneratorKeys.Cer);
            services.AddKeyedTransient<IFileGenerator, RSAPemFileGenerator>(FileGeneratorKeys.Rsa);
            services.AddSingleton<IKeyVaultExportFiles, KeyVaultExportFiles>();
            services.AddTransient<IKeyVaultCompareService, KeyVaultCompareService>();

        }


        static void AddView<
           [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
           [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel
           >(this IServiceCollection services)
           where TView : View
           where TViewModel : ViewModel

        {
            services.AddScoped<TView>();
            services.AddScoped<TViewModel>();

        }


        public static void AddViews(this IServiceCollection services)
        {
            services.AddScoped<ViewModelContext>();


            services.AddView<RsaView, RsaViewModel>();
            services.AddView<CerFileView, CerFileViewModel>();
            services.AddView<MainView, MainViewModel>();
            services.AddView<LoginView, LoginViewModel>();
            services.AddView<AppWizardView, AppWizardViewModel>();

            services.AddView<KeyVaultSecretMainView, KeyVaultSecretMainViewModel>();
            services.AddView<ProjectManagementView, ProjectManagementViewModel>();
            services.AddView<OptionsView, OptionsViewModel>();
            services.AddView<ContainerRegistryMainView, ContainerRegistryMainViewModel>();
            services.AddView<AppContainerMainView, AppContainerMainViewModel>();
            services.AddView<TenantManagementView, TenantManagementViewModel>();

        }
    }
}