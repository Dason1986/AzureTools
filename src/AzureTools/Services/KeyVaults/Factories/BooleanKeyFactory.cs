using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class BooleanKeyFactory : KeyFactory
    {
        public override string Type => "Boolean";
        public override IKeyValidator CreateValidator() => new BooleanKeyValidator();
    }
}
