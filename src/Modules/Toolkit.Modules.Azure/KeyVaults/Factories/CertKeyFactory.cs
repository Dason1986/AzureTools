using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class CertKeyFactory : KeyFactory
    {
        public override string Type => "Cert";
        public override IKeyValidator CreateValidator() => new CertKeyValidator();
    }
}
