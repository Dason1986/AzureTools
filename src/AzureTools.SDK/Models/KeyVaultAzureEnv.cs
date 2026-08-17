namespace AzureTools.SDK.Models
{
    public class KeyVaultAzureEnv : AzureEnv
    {
        public string KeyVault { get; set; }
        public string KeyVaultUrl { get; set; }

        public bool? EnableSoftDelete { get; set; }
    }
}
