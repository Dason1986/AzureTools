namespace AzureTools.Models
{
    public class ContainerRegistryEnv : AzureEnvOption
    {

        public string Registry { get; set; }
        public string RegistryUrl { get; set; }
        public bool? IsAdminUserEnabled { get; set; }
        public bool? IsAnonymousPullEnabled { get; set; }

    }
}
