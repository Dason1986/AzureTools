using AzureTools.Properties;
using System;

namespace AzureTools.Services.KeyVaults.Validators
{
    public class GuidKeyValidator : IKeyValidator
    {
        public string Type => "Guid";
        public string ErrorMessage => MessageResources.ValidatorGuidInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return Guid.TryParse(value, out _);
        }
    }
}
