namespace AzureTools.Cli
{
    public interface ISshChainBuilder : ICliClientBuilder
    {
        /// <summary>选定容器，返回操作器</summary>
        IContainerClient WithContainer(string containerName);

    }

}
