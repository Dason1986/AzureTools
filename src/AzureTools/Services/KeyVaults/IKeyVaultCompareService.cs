
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureTools.Services.KeyVaults
{
    public interface IKeyVaultCompareService
    {
        Task<List<SecretCompareReport>> CompareAsync(params SDK.Models.KeyVaultAzureEnv[] options);
    }
}
