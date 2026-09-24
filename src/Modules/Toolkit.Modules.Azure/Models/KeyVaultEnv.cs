namespace Toolkit.Modules.Azure.Models
{
    public class KeyVaultEnv : AzureEnvOption
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        public string KeyVault { get; set; }
        public string KeyVaultUrl { get; set; }

        public bool? EnableSoftDelete { get; set; }

        #endregion
    }
}
