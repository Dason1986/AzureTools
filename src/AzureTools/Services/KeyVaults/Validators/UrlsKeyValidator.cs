using AzureTools.Properties;
using System;
using System.Linq;

namespace AzureTools.Services.KeyVaults.Validators
{
    public class UrlsKeyValidator : IKeyValidator
    {
        public string Type => "Urls";
        public string ErrorMessage => MessageResources.ValidatorUrlsInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string[] urls = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (urls.Length == 0) return false;
            return urls.All(u => Uri.TryCreate(u.Trim(), UriKind.Absolute, out _));
        }
    }
}
