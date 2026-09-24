using System;
using System.Collections.Generic;

namespace AzureTools.ViewModels
{
    public sealed class ViewParameters
    {
        private readonly Dictionary<string, object> _values = new(StringComparer.OrdinalIgnoreCase);

        public ViewParameters Set(string name, object value)
        {
            _values[name] = value;
            return this;
        }

        public T Get<T>(string name)
        {
            if (!_values.TryGetValue(name, out object value))
                throw new KeyNotFoundException($"缺少视图参数：{name}");
            return (T)value;
        }

        public T GetOrDefault<T>(string name, T defaultValue = default)
        {
            return _values.TryGetValue(name, out object value) ? (T)value : defaultValue;
        }
    }
}
