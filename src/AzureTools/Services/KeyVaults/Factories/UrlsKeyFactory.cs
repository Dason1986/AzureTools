using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class UrlsKeyFactory : KeyFactory
    {
        public override string Type => "Urls";
        public override IKeyValidator CreateValidator() => new UrlsKeyValidator();
    }
}
