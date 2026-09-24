using AzureTools.SDK;
using Toolkit.Modules.Models;

namespace Toolkit.Modules.Azure
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

        public string[] GetSecretKeyTypes()
        {
            return ["", "Boolean", "Guid", "Number", "Pfx", "Pfx_Password", "Rsa", "Url", "Urls"];
        }

    }
}
