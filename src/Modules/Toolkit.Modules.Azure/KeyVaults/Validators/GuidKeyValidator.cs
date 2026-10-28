using System;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class GuidKeyValidator : IKeyValidator
    {
        public string Type => "Guid";
        public string ErrorMessage => AzureResource.ValidatorGuidInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Guid.TryParse(value, out _);
        }
    }
}
