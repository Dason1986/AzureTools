using AzureTools.SDK.KeyVaults;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.Models.KeyVaults;

namespace AzureTools.Tests.Stubs
{
    public static class StubKeyVaultManagement
    {
        public static Mock<IKeyVaultManagement> Create(
            string vaultName = "testvault",
            List<SecretItem> keys = null,
            SecretKeyOptionItem[] requiredKeys = null)
        {
            Mock<IKeyVaultManagement> mock = new Mock<IKeyVaultManagement>();
            mock.Setup(m => m.KeyVaultName).Returns(vaultName);
            mock.Setup(m => m.GetKeys())
                .ReturnsAsync(keys ?? new List<SecretItem>());
            mock.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string key, string version) => keys?.FirstOrDefault(x => x.Name == key)?.Value);
            mock.Setup(m => m.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.GetDeletedKeys())
                .ReturnsAsync(new List<SecretItem>());
            mock.Setup(m => m.GetKeyVersion(It.IsAny<SecretItem>()))
                .ReturnsAsync((SecretItem s) => new List<SecretItem> { s });
            mock.Setup(m => m.SetExpiration(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.CompletedTask);
            return mock;
        }
    }
}
