using AzureTools.SDK.KeyVaults;
using Moq;
using System.Collections.Generic;
using Toolkit.Modules.Azure.Stores;

namespace AzureTools.Tests.Stubs
{
    public static class SubKeyVaultStore
    {
        public static Mock<IKeyVaultStore> Create()
        {
            Mock<IKeyVaultStore> mock = new Mock<IKeyVaultStore>(); List<SecretItem> keys = new List<SecretItem>
            {
                new() { Name = "key1",   Value = "val1" },
                new() { Name = "key2",  Value = "val2" }
            };


            return mock;
        }

    }
}
