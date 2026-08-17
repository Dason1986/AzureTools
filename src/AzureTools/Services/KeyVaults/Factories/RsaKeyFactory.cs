using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class RsaKeyFactory : KeyFactory
    {
        public override string Type => "Rsa";
        public override IKeyValidator CreateValidator() => new RsaKeyValidator();
    }
}
