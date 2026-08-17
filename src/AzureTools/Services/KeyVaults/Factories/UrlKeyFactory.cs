using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class UrlKeyFactory : KeyFactory
    {
        public override string Type => "Url";
        public override IKeyValidator CreateValidator() => new UrlKeyValidator();
    }
}
