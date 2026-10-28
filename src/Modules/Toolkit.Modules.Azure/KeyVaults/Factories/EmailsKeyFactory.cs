using Toolkit.Modules.Azure.KeyVaults.Validators;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class EmailsKeyFactory : KeyFactory
    {
        public override string Type => "Emails";
        public override IKeyValidator CreateValidator() => new EmailsKeyValidator();
    }
}
