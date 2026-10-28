using System.Collections.Generic;
using System.Text.Json.Serialization;
using Toolkit.Modules.SSH.ContainerRuntime.Models;
using Toolkit.Modules.SSH.ContainerRuntime.Models.Http;
using Toolkit.Modules.SSH.Models;
using Toolkit.Modules.SSH.Models.Features;

namespace Toolkit.Modules.SSH
{
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(SshHostProfile))]
    [JsonSerializable(typeof(List<SshHostProfile>))]
    [JsonSerializable(typeof(Container))]
    [JsonSerializable(typeof(List<Container>))]
    [JsonSerializable(typeof(Image))]
    [JsonSerializable(typeof(ImageInspect))]
    [JsonSerializable(typeof(List<ImageInspect>))]
    [JsonSerializable(typeof(ContainerCreateConfig))]
    [JsonSerializable(typeof(Volume))]
    [JsonSerializable(typeof(Network))]
    [JsonSerializable(typeof(List<Network>))]
    [JsonSerializable(typeof(Pod))]
    [JsonSerializable(typeof(HttpVolumeList))]
    [JsonSerializable(typeof(HttpVolumeCreateRequest))]
    [JsonSerializable(typeof(HttpNetworkCreateRequest))]
    [JsonSerializable(typeof(HttpNetworkInspect))]
    [JsonSerializable(typeof(List<HttpNetworkInspect>))]
    [JsonSerializable(typeof(HttpNetworkConnectRequest))]
    [JsonSerializable(typeof(List<HttpContainer>))]
    [JsonSerializable(typeof(List<HttpImage>))]
    [JsonSerializable(typeof(List<HttpRegistryImage>))]
    [JsonSerializable(typeof(HttpCreateResponse))]
    [JsonSerializable(typeof(HttpContainerCreateRequest))]
    [JsonSerializable(typeof(HttpHostConfig))]
    [JsonSerializable(typeof(HttpRestartPolicy))]
    [JsonSerializable(typeof(HttpPortBinding))]
    [JsonSerializable(typeof(DockerFeatureConfig))]
    [JsonSerializable(typeof(PodmanFeatureConfig))]
    [JsonSerializable(typeof(TerminalFeatureConfig))]
    [JsonSerializable(typeof(PackageFeatureConfig))]
    [JsonSerializable(typeof(FirewallFeatureConfig))]
    [JsonSerializable(typeof(FileBrowserFeatureConfig))]
    [JsonSerializable(typeof(ContainerStats))]
    [JsonSerializable(typeof(HttpContainerStats))]
    [JsonSerializable(typeof(HttpCpuStats))]
    [JsonSerializable(typeof(HttpCpuUsage))]
    [JsonSerializable(typeof(HttpMemoryStats))]
    [JsonSerializable(typeof(HttpBlkioStats))]
    [JsonSerializable(typeof(HttpBlkioEntry))]
    [JsonSerializable(typeof(HttpNetworkStats))]
    public partial class SshJsonSerializerContext : JsonSerializerContext
    {
    }
}
