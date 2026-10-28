using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolkit.Modules.SSH.Models;

namespace Toolkit.Modules.SSH.ViewModels.SshHosts
{
    public abstract partial class SshFeatureTabViewModel : ObservableObject
    {
        public abstract string Code { get; }

        public abstract string DisplayName { get; }

        [ObservableProperty]
        private bool _isEnabled;

        protected bool IsFeatureEnabled(Dictionary<string, string> features)
        {
            return features != null && features.ContainsKey(Code);
        }

        protected static TConfig DeserializeConfig<TConfig>(Dictionary<string, string> features, string code, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TConfig> typeInfo) where TConfig : class, new()
        {
            if (features == null || !features.TryGetValue(code, out string json))
                return new TConfig();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize(json, typeInfo) ?? new TConfig();
            }
            catch
            {
                return new TConfig();
            }
        }

        protected static void SerializeConfig<TConfig>(Dictionary<string, string> features, string code, TConfig config, System.Text.Json.Serialization.Metadata.JsonTypeInfo<TConfig> typeInfo) where TConfig : class
        {
            if (features == null) return;

            string json = System.Text.Json.JsonSerializer.Serialize(config, typeInfo);
            features[code] = json;
        }

        protected static void RemoveConfig(Dictionary<string, string> features, string code)
        {
            features?.Remove(code);
        }

        public abstract Task LoadAsync(SshHostProfile host);

        public abstract void Apply(SshHostProfile host);
    }
}
