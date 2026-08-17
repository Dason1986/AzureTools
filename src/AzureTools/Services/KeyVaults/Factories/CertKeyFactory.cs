using AzureTools.Services.KeyVaults.Validators;

namespace AzureTools.Services.KeyVaults.Factories
{
    public class CertKeyFactory : KeyFactory
    {
        public override string Type => "Cert";
        public override IKeyValidator CreateValidator() => new CertKeyValidator();
    }
}
