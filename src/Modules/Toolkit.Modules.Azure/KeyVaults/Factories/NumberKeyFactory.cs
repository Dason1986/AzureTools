using AzureTools.Extensions;
using System.Text.Json;
using Toolkit.Modules.Azure.KeyVaults.Validators;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class NumberKeyFactory : KeyFactory
    {
        public override string Type => "Number";
        public override IKeyValidator CreateValidator() => new NumberKeyValidator();

        public override ValidationOptions CreateOptions(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return new NumberValidationOptions { Type = Type };
            string errorMsg = element.GetString("ErrorMessage");
            NumberValidationOptions options = new NumberValidationOptions
            {
                Type = Type,
                ErrorMessage = errorMsg,
                Min = element.GetDouble("Min"),
                Max = element.GetDouble("Max")
            };

            return options;
        }
    }
}
