using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public partial class EnvEntry : ObservableObject
    {
        [ObservableProperty]
        private string _key;

        [ObservableProperty]
        private string _value;

        public Func<string, string> DefaultValueResolver { get; set; }

        public Func<string, string, string, Task<bool>> ValueConflictResolver { get; set; }

        partial void OnKeyChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            string defaultValue = DefaultValueResolver?.Invoke(value);
            if (defaultValue == null) return;

            if (string.IsNullOrEmpty(Value))
            {
                Value = defaultValue;
                return;
            }

            if (!string.Equals(Value, defaultValue, StringComparison.Ordinal))
            {
                _ = ResolveConflictAsync(value, defaultValue);
            }
        }

        private async Task ResolveConflictAsync(string key, string defaultValue)
        {
            if (ValueConflictResolver == null) return;
            if (await ValueConflictResolver(key, Value, defaultValue))
            {
                Value = defaultValue;
            }
        }
    }
}
