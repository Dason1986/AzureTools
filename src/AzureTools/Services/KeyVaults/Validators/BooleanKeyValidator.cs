using AzureTools.Extensions;
using AzureTools.Properties;

namespace AzureTools.Services.KeyVaults.Validators
{
    public class BooleanKeyValidator : IKeyValidator
    {
        public string Type => "Boolean";

        public string ErrorMessage => MessageResources.ValidatorBooleanInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string trimmed = value.Trim();
            return StructExtensions.IsTrue(trimmed) || StructExtensions.IsFalse(trimmed);
        }
    }
}
