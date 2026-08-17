using AzureTools.Models.KeyVaults;
using AzureTools.SDK.KeyVaults;
using AzureTools.Services.KeyVaults.Factories;
using AzureTools.Services.KeyVaults.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AzureTools.Services.KeyVaults
{
    public class KeyValidatorProvider
    {
        public KeyValidatorProvider(IEnumerable<KeyFactory> factories)
        {
            _factories = factories.ToDictionary(f => f.Type, StringComparer.OrdinalIgnoreCase);
        }

        private readonly Dictionary<string, KeyFactory> _factories;

        public static KeyValidatorProvider Default { get; } = new KeyValidatorProvider(new KeyFactory[]
        {
            new RsaKeyFactory(),
            new CertKeyFactory(),
            new PfxKeyFactory(),
            new UrlKeyFactory(),
            new UrlsKeyFactory(),
            new BooleanKeyFactory(),
            new GuidKeyFactory(),
            new NumberKeyFactory(),
            new PfxPasswordKeyFactory()
        });

        public IKeyFactory GetFactory(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            _factories.TryGetValue(key, out KeyFactory factory);
            return factory;
        }

        public ValidationOptions CreateOptions(string type, JsonElement element)
        {
            if (!string.IsNullOrEmpty(type) && _factories.TryGetValue(type, out KeyFactory factory))
                return factory.CreateOptions(element);

            return new DefaultValidationOptions { Type = type };
        }

        public bool Validate(ValidationOptions options, string value, IEnumerable<SecretItem> allItems = null)
        {
            if (options == null || string.IsNullOrEmpty(value)) return true;

            ValidatorContext context = CreateContext(options, allItems);
            IKeyValidator validator = GetValidator(options.Type);
            return validator?.Validate(value, context) ?? true;
        }

        public IKeyValidator GetValidator(string Type)
        {
            if (string.IsNullOrEmpty(Type)) return null;
            return _factories.TryGetValue(Type, out KeyFactory factory) ? factory.CreateValidator() : null;
        }

        public ValidatorContext CreateContext(ValidationOptions options, IEnumerable<SecretItem> allItems = null)
        {
            ValidatorContext context = new ValidatorContext(options);

            if (allItems != null && options is IPasswordValidationOptions pfxOpt && !string.IsNullOrEmpty(pfxOpt.PasswordKey))
            {
                SecretItem passwordItem = allItems.FirstOrDefault(x => x.Name == pfxOpt.PasswordKey);
                if (passwordItem != null)
                    context.Set("password", passwordItem.Value);
            }

            return context;
        }

        public string GetErrorMessage(ValidationOptions options)
        {
            if (options != null && !string.IsNullOrEmpty(options.ErrorMessage))
                return options.ErrorMessage;

            IKeyValidator validator = GetValidator(options?.Type);
            return validator.ErrorMessage ?? "验证失败";
        }
    }
}
