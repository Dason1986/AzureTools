using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;

namespace Toolkit.Modules.SSH
{
    public static class SshHostProfileExtensions
    {
        public static DockerFeatureConfig GetDockerConfig(this SshHostProfile host)
            => GetFeatureConfig(host, SshHostFeature.Docker, SshJsonSerializerContext.Default.DockerFeatureConfig);

        public static PodmanFeatureConfig GetPodmanConfig(this SshHostProfile host)
            => GetFeatureConfig(host, SshHostFeature.Podman, SshJsonSerializerContext.Default.PodmanFeatureConfig);

        private static TConfig GetFeatureConfig<TConfig>(SshHostProfile host, string code, JsonTypeInfo<TConfig> typeInfo) where TConfig : new()
        {
            if (host?.Features == null || !host.Features.TryGetValue(code, out string json))
                return new TConfig();

            try
            {
                return JsonSerializer.Deserialize(json, typeInfo) ?? new TConfig();
            }
            catch
            {
                return new TConfig();
            }
        }

        public static void MigrateLegacyData(this SshHostProfile host)
        {
            if (host == null || host.LegacyData == null || host.LegacyData.Count == 0) return;

            bool needMigrate = false;

            if (host.LegacyData.TryGetValue("EnabledFeatures", out JsonElement enabledFeaturesElem)
                && enabledFeaturesElem.ValueKind == JsonValueKind.Array)
            {
                needMigrate = true;
                MigrateEnabledFeatures(host, enabledFeaturesElem);
            }

            if (host.LegacyData.TryGetValue("DockerPort", out JsonElement dockerPortElem)
                && host.LegacyData.TryGetValue("DockerApiMode", out JsonElement dockerApiModeElem))
            {
                needMigrate = true;
                MigrateDockerConfig(host, dockerPortElem, dockerApiModeElem);
            }

            if (needMigrate)
            {
                host.LegacyData = null;
            }
        }

        private static void MigrateEnabledFeatures(SshHostProfile host, JsonElement enabledFeaturesElem)
        {
            if (host.Features == null) host.Features = new Dictionary<string, string>();

            foreach (JsonElement featureElem in enabledFeaturesElem.EnumerateArray())
            {
                if (featureElem.ValueKind != JsonValueKind.String) continue;
                string code = featureElem.GetString();
                if (string.IsNullOrEmpty(code)) continue;

                if (SshHostFeature.Docker == code)
                {
                    MigrateDockerConfigFromLegacy(host);
                }
                else if (!host.Features.ContainsKey(code))
                {
                    host.Features[code] = "{}";
                }
            }
        }

        private static void MigrateDockerConfigFromLegacy(SshHostProfile host)
        {
            if (host.LegacyData.TryGetValue("DockerPort", out JsonElement portElem)
                && host.LegacyData.TryGetValue("DockerApiMode", out JsonElement modeElem))
            {
                MigrateDockerConfig(host, portElem, modeElem);
            }
            else if (!host.Features.ContainsKey(SshHostFeature.Docker))
            {
                host.Features[SshHostFeature.Docker] = JsonSerializer.Serialize(new DockerFeatureConfig(), SshJsonSerializerContext.Default.DockerFeatureConfig);
            }
        }

        private static void MigrateDockerConfig(SshHostProfile host, JsonElement portElem, JsonElement modeElem)
        {
            if (host.Features == null) host.Features = new Dictionary<string, string>();

            DockerFeatureConfig config = new DockerFeatureConfig();
            if (portElem.ValueKind == JsonValueKind.Number)
                config.DockerPort = portElem.GetInt32();
            if (modeElem.ValueKind == JsonValueKind.String)
                config.DockerApiMode = modeElem.GetString();

            host.Features[SshHostFeature.Docker] = JsonSerializer.Serialize(config, SshJsonSerializerContext.Default.DockerFeatureConfig);
        }
    }
}
