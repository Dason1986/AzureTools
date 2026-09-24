using System;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
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
