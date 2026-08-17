using AzureTools.Extensions;
using AzureTools.Models.KeyVaults;
using AzureTools.Services.KeyVaults.Validators;
using System.Text.Json;

namespace AzureTools.Services.KeyVaults.Factories
{
    public abstract class KeyFactory : IKeyFactory
    {
        public abstract string Type { get; }
        public abstract IKeyValidator CreateValidator();

        public virtual ValidationOptions CreateOptions(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Undefined) return new DefaultValidationOptions { Type = Type };
            string errorMsg = element.GetString("ErrorMessage");
            return new DefaultValidationOptions
            {
                Type = Type,
                ErrorMessage = errorMsg
            };
        }

        public virtual ValidationOptions CreateOptions(string json)
        {
            if (string.IsNullOrEmpty(json))
                return CreateOptions(new JsonElement());

            using JsonDocument doc = JsonDocument.Parse(json);
            return CreateOptions(doc.RootElement);
        }
    }
}
