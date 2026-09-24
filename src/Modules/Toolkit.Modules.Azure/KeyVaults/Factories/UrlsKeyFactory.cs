using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class UrlsKeyFactory : KeyFactory
    {
        public override string Type => "Urls";
        public override IKeyValidator CreateValidator() => new UrlsKeyValidator();
    }
}
