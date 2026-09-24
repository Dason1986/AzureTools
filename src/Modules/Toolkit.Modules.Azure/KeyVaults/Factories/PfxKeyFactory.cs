



using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class PfxKeyFactory : KeyFactory
    {
        public override string Type => "Pfx";
        public override IKeyValidator CreateValidator() => new PfxKeyValidator();
    }
}
