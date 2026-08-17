namespace AzureTools.SDK.ContainerRegistries
{
    public class RepositoryInfo
    {
        public string Name { get; set; }
        public string RegistryEndpoint { get; set; }
        public ManifestProperty[] ManifestProperties { get; set; }
    }
}
