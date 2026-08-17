namespace AzureTools.SDK.Models
{
    public class ContainerRegistryAzureEnv : AzureEnv
    {
        public string Registry { get; set; }
        public string RegistryUrl { get; set; }
        public bool? IsAdminUserEnabled { get; set; }
        public bool? IsAnonymousPullEnabled { get; set; }
    }
}
