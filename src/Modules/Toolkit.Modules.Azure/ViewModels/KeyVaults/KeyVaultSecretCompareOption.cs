
using System.Collections.Generic;
using Toolkit.Modules.Azure.KeyVaults;

namespace Toolkit.Modules.Azure.ViewModels.KeyVaults
{
    public class KeyVaultSecretCompareOption
    {
        public List<string> EnvNames { get; set; }

        public List<SecretCompareReport> Results { get; set; }
    }
}
