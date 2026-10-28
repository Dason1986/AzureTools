using System;
using System.Linq;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class UrlsKeyValidator : IKeyValidator
    {
        public string Type => "Urls";
        public string ErrorMessage => AzureResource.ValidatorUrlsInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string[] urls = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (urls.Length == 0) return false;
            return urls.All(u => Uri.TryCreate(u.Trim(), UriKind.Absolute, out _));
        }
    }
}
