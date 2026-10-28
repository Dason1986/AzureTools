using System;
using Toolkit.Modules.Azure.Models.KeyVaults;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class NumberKeyValidator : IKeyValidator
    {
        public string Type => "Number";

        public string ErrorMessage => AzureResource.ValidatorNumberInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (!double.TryParse(value, out double num)) return false;

            if (context?.Options is IRangeValidationOptions<double> numOpt)
            {

                if (numOpt.Min.HasValue && num < numOpt.Min.Value)
                    throw new NotSupportedException(string.Format(AzureResource.ValidatorNumberMinExceeded, numOpt.Min));
                if (numOpt.Max.HasValue && num > numOpt.Max.Value)
                    throw new NotSupportedException(string.Format(AzureResource.ValidatorNumberMaxExceeded, numOpt.Max));
            }

            return true;
        }
    }
}
