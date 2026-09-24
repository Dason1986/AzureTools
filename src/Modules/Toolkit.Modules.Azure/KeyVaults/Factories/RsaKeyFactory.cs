using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class RsaKeyFactory : KeyFactory
    {
        public override string Type => "Rsa";
        public override IKeyValidator CreateValidator() => new RsaKeyValidator();
    }
}
