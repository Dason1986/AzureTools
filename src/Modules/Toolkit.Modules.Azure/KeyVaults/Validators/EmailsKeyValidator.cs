using System;
using System.Linq;
using System.Net.Mail;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class EmailsKeyValidator : IKeyValidator
    {
        public string Type => "Emails";
        public string ErrorMessage => AzureResource.ValidatorEmailsInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string[] emails = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (emails.Length == 0) return false;
            return emails.All(e => MailAddress.TryCreate(e.Trim(), out _));
        }
    }
}
