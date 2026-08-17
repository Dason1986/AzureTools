using System.Threading.Tasks;

namespace AzureTools.SDK.AppContainers
{
    public interface IAppContainerManagement
    {
        string EnvironmentName { get; }

        Task<ContainerAppInfo[]> GetContainerAppsAsync();

        Task<ContainerAppDetail> GetContainerAppDetailAsync(string containerAppName);
    }
}
