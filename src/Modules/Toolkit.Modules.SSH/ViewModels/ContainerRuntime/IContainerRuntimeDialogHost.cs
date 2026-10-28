using System.Threading.Tasks;

namespace Toolkit.Modules.SSH.ViewModels.ContainerRuntime
{
    public interface IContainerRuntimeDialogHost
    {
        Task ShowContainerDetailAsync(ContainerDetailOption option);

        Task ShowContainerMonitorAsync(ContainerMonitorOption option);

        Task ShowContainerCreateAsync(ContainerCreateOption option);

        Task ShowImageDetailAsync(ImageDetailOption option);

        Task ShowImageTagsAsync(ImageTagsOption option);

        Task ShowImageSearchAsync(ImageSearchOption option);

        Task ShowNetworkDetailAsync(NetworkDetailOption option);

        Task ShowPodCreateAsync(PodCreateOption option);

        Task ShowPodEditAsync(PodEditOption option);
    }
}
