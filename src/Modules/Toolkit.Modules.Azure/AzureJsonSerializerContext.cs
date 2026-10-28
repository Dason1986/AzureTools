using System.Collections.Generic;
using System.Text.Json.Serialization;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace Toolkit.Modules.Azure
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
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
    public partial class AzureJsonSerializerContext : JsonSerializerContext
    {
    }
}
