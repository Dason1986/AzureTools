using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class GuidKeyFactory : KeyFactory
    {
        public override string Type => "Guid";
        public override IKeyValidator CreateValidator() => new GuidKeyValidator();
    }
}
