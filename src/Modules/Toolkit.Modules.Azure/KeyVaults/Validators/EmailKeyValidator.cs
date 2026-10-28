using System.Net.Mail;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class EmailKeyValidator : IKeyValidator
    {
        public string Type => "Email";
        public string ErrorMessage => AzureResource.ValidatorEmailInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return MailAddress.TryCreate(value, out _);
        }
    }
}
