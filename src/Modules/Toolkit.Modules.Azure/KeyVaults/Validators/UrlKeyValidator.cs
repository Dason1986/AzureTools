using System;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class UrlKeyValidator : IKeyValidator
    {
        public string Type => "Url";
        public string ErrorMessage => MessageResources.ValidatorUrlInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Uri.TryCreate(value, UriKind.Absolute, out _);
        }
    }
}
