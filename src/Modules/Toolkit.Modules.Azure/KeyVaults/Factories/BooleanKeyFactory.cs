using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class BooleanKeyFactory : KeyFactory
    {
        public override string Type => "Boolean";
        public override IKeyValidator CreateValidator() => new BooleanKeyValidator();
    }
}
