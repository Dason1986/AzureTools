using System;
using System.Collections.Generic;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure.KeyVaults
{
    public class ValidatorContext
    {
        public ValidatorContext(ValidationOptions options)
        {
            Options = options;
        }

        public ValidationOptions Options { get; }
        public Dictionary<string, object> Parameters { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void Set(string key, object value)
        {
            Parameters[key] = value;
        }

        public T Get<T>(string key)
        {
            if (Parameters.TryGetValue(key, out object value) && value is T typed)
                return typed;
            return default;
        }

        public bool Has(string key) => Parameters.ContainsKey(key);
    }
}
