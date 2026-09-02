using AzureTools.Models;
using AzureTools.Properties;
using AzureTools.SDK;

namespace AzureTools.Services
{
    public class OptionFactory
    {
        public static readonly OptionFactory Instance = new OptionFactory();
        public Option<AzureCredential>[] GetCredentials()
        {
            return
            [
                new Option<AzureCredential>(AzureCredential.Browser, "浏览器"),
                new Option<AzureCredential>(AzureCredential.DeviceCode, "浏览器+授权码"),
                new Option<AzureCredential>(AzureCredential.AzureCli, "Azure Cli"),
                new Option<AzureCredential>(AzureCredential.Certificate, "证书"),
                new Option<AzureCredential>(AzureCredential.CertificatePassword, "证书+密码")
            ];
        }
        public Option<TenantMode>[] GetTenantModes()
        {
            return
            [
                new Option<TenantMode>(TenantMode.Single, "单租户"),
                new Option<TenantMode>(TenantMode.Multiple, "多租户")
            ];
        }
        public Option<ThemeMode>[] GetThemeModes()
        {
            return
            [
                new Option<ThemeMode>(ThemeMode.System, FormResources.ThemeSystem),
                new Option<ThemeMode>(ThemeMode.Light, FormResources.ThemeLight),
                new Option<ThemeMode>(ThemeMode.Dark, FormResources.ThemeDark)
            ];
        }
        public string[] GetSecretKeyTypes()
        {
            return ["", "Boolean", "Guid", "Number", "Pfx", "Pfx_Password", "Rsa", "Url", "Urls"];
        }

    }
}
