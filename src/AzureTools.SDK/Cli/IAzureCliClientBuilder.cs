namespace AzureTools.Cli
{
    public interface IAzureCliClientBuilder : ICliClientBuilder
    {
        IAppContainerClient GetAppContainerClient(string name, string resourceGroup);

        IContainerRegistrieClient GetContainerRegistrieClient(string name, string resourceGroup);
    }
}
