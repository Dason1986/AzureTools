
using AzureTools.SDK.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toolkit.Modules.Azure.KeyVaults
{
    public interface IKeyVaultCompareService
    {
        Task<List<SecretCompareReport>> CompareAsync(params KeyVaultAzureEnv[] options);
    }
}
