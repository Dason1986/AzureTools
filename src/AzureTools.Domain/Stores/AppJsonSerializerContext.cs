using AzureTools.Models;
using AzureTools.Models.KeyVaults;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureTools.Stores
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(SettingOptioinItem))]
    [JsonSerializable(typeof(List<SettingOptioinItem>))]
    [JsonSerializable(typeof(TenantProfile))]
    [JsonSerializable(typeof(AppProfile))]
    [JsonSerializable(typeof(List<AppProfile>))]
    [JsonSerializable(typeof(List<TenantProfile>))]
    [JsonSerializable(typeof(ProjectInfo))]
    [JsonSerializable(typeof(List<ProjectInfo>))]
    [JsonSerializable(typeof(KeyVaultEnv))]
    [JsonSerializable(typeof(List<KeyVaultEnv>))]
    [JsonSerializable(typeof(ContainerRegistryEnv))]
    [JsonSerializable(typeof(List<ContainerRegistryEnv>))]
    [JsonSerializable(typeof(AppContainerEnv))]
    [JsonSerializable(typeof(List<AppContainerEnv>))]
    [JsonSerializable(typeof(SecretKeyOptionItem))]
    [JsonSerializable(typeof(List<SecretKeyOptionItem>))]
    [JsonSerializable(typeof(ValidationOptions))]
    [JsonSerializable(typeof(DefaultValidationOptions))]
    [JsonSerializable(typeof(PfxPasswordValidationOptions))]
    [JsonSerializable(typeof(NumberValidationOptions))]
    public partial class AppJsonSerializerContext : JsonSerializerContext
    {
    }
}
