
using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class GuidKeyFactory : KeyFactory
    {
        public override string Type => "Guid";
        public override IKeyValidator CreateValidator() => new GuidKeyValidator();
    }
}
