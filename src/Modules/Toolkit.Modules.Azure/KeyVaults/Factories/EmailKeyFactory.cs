using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class EmailKeyFactory : KeyFactory
    {
        public override string Type => "Email";
        public override IKeyValidator CreateValidator() => new EmailKeyValidator();
    }
}
