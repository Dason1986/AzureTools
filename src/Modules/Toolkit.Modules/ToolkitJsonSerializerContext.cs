using System.Collections.Generic;
using System.Text.Json.Serialization;
using Toolkit.Modules.Models;

namespace Toolkit.Modules
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(SettingOptioinItem))]
    [JsonSerializable(typeof(List<SettingOptioinItem>))]
    [JsonSerializable(typeof(TenantProfile))]
    [JsonSerializable(typeof(List<TenantProfile>))]
    [JsonSerializable(typeof(AppProfile))]
    [JsonSerializable(typeof(List<AppProfile>))]
    [JsonSerializable(typeof(ProjectInfo))]
    [JsonSerializable(typeof(List<ProjectInfo>))]
    public partial class ToolkitJsonSerializerContext : JsonSerializerContext
    {
    }
}
