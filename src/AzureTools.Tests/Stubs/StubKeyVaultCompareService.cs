using AzureTools.Services.KeyVaults;
using Moq;
using System.Collections.Generic;

namespace AzureTools.Tests.Stubs
{
    public static class StubKeyVaultCompareService
    {
        public static Mock<IKeyVaultCompareService> Create(
            List<SecretCompareReport> results = null)
        {
            Mock<IKeyVaultCompareService> mock = new Mock<IKeyVaultCompareService>();
            mock.Setup(m => m.CompareAsync(It.IsAny<SDK.Models.KeyVaultAzureEnv[]>()))
                .ReturnsAsync(results ?? new List<SecretCompareReport>());
            return mock;
        }
    }
}
