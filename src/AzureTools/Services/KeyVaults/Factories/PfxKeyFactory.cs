
using AzureTools.Services.KeyVaults.Validators;


namespace AzureTools.Services.KeyVaults.Factories
{
    public class PfxKeyFactory : KeyFactory
    {
        public override string Type => "Pfx";
        public override IKeyValidator CreateValidator() => new PfxKeyValidator();
    }
}
