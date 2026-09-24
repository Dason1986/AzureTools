using AzureTools.Extensions;
using System.Text.Json;
using Toolkit.Modules.Azure.KeyVaults.Validators;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure.KeyVaults.Factories
{
    public class PfxPasswordKeyFactory : KeyFactory
    {
        public override string Type => "Pfx_Password";
        public override IKeyValidator CreateValidator() => new PfxKeyValidator();

        public override ValidationOptions CreateOptions(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return new PfxPasswordValidationOptions { Type = Type };
            string passwordKey = element.GetString("PasswordKey");
            string errorMsg = element.GetString("ErrorMessage");
            return new PfxPasswordValidationOptions
            {
                Type = Type,
                PasswordKey = passwordKey,
                ErrorMessage = errorMsg
            };
        }
    }
}
