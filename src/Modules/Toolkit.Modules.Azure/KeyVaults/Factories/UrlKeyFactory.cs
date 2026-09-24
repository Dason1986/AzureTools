using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class UrlKeyFactory : KeyFactory
    {
        public override string Type => "Url";
        public override IKeyValidator CreateValidator() => new UrlKeyValidator();
    }
}
