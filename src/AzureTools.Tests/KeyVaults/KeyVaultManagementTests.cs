using Azure;
using Azure.Security.KeyVault.Secrets;
using AzureTools.SDK.KeyVaults;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTools.Tests.KeyVaults
{
    public class KeyVaultManagementTests
    {
        private Mock<SecretClient> _mockClient = null!;
        private KeyVaultManagement _management = null!;

        [SetUp]
        public void SetUp()
        {
            _mockClient = new Mock<SecretClient>();
            _management = new KeyVaultManagement("testvault", null, _mockClient.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.CurrentDirectory = Path.Combine(AppContext.BaseDirectory);
        }

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidName_SetsKeyVaultName()
        {
            Assert.That(_management.KeyVaultName, Is.EqualTo("testvault"));
        }

        [Test]
        public void Constructor_WithNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new KeyVaultManagement(null!, null));
        }

        [Test]
        public void Constructor_WithEmptyName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new KeyVaultManagement("", null));
        }

        [Test]
        public void Constructor_WithClient_UsesInjectedClient()
        {
            Assert.That(_management.KeyVaultName, Is.EqualTo("testvault"));
        }

        #endregion



        #region Get Tests

        [Test]
        public async Task Get_ExistingKey_ReturnsValue()
        {
            string expectedValue = "test-value";
            SetupGetSecret("testkey", expectedValue);

            string result = await _management.Get("testkey");

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task Get_MockClient_ReturnsResponse()
        {
            string expectedValue = "test-value";
            KeyVaultSecret secret = new KeyVaultSecret("testkey", expectedValue);
            Response<KeyVaultSecret> mockResponse = Response.FromValue(secret, null);

            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(mockResponse));

            Response<KeyVaultSecret> response = await _mockClient.Object.GetSecretAsync("testkey", null, CancellationToken.None);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Value, Is.Not.Null);
            Assert.That(response.Value.Value, Is.EqualTo(expectedValue));
        }

        [Test]
        public async Task Get_MockClient_TwoParameters()
        {
            string expectedValue = "test-value";
            KeyVaultSecret secret = new KeyVaultSecret("testkey", expectedValue);
            Response<KeyVaultSecret> mockResponse = Response.FromValue(secret, null);

            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(mockResponse));

            Response<KeyVaultSecret> response = await _mockClient.Object.GetSecretAsync("testkey", null);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Value, Is.Not.Null);
            Assert.That(response.Value.Value, Is.EqualTo(expectedValue));
        }


        [Test]
        public async Task Get_WithVersion_ReturnsVersionedValue()
        {
            string expectedValue = "versioned-value";
            SetupGetSecret("testkey", "v1.0", expectedValue);

            string result = await _management.Get("testkey", "v1.0");

            Assert.That(result, Is.EqualTo(expectedValue));
        }

        #endregion

        #region Set Tests

        [Test]
        public async Task Set_ValidKeyAndValue_CallsSetSecretAsync()
        {
            _mockClient.Setup(c => c.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(Response.FromValue(new KeyVaultSecret("testkey", "testvalue"), null)));
            _mockClient.Setup(c => c.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(Response.FromValue(new KeyVaultSecret("testkey", "testvalue"), null)));

            await _management.Set("testkey", "testvalue");

            _mockClient.Verify(c => c.SetSecretAsync("testkey", "testvalue", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Set_EmptyKey_ThrowsArgumentExceptionAsync()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _management.Set("", "value"));
        }

        [Test]
        public async Task Set_EmptyValue_ThrowsArgumentExceptionAsync()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _management.Set("key", ""));
        }

        [Test]
        public async Task Set_NullKey_ThrowsArgumentNullExceptionAsync()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _management.Set(null!, "value"));
        }

        [Test]
        public async Task Set_NullValue_ThrowsArgumentNullExceptionAsync()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _management.Set("key", null!));
        }

        #endregion

        #region GetKeys Tests

        [Test]
        public async Task GetKeys_ReturnsAllSecrets()
        {
            List<SecretProperties> secrets = new List<SecretProperties>
            {
                new SecretProperties("key1"),
                new SecretProperties("key2")
            };

            SetupGetPropertiesOfSecrets(secrets);

            List<SecretItem> result = await _management.GetKeys();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetKeys_EmptyVault_ReturnsEmptyList()
        {
            SetupGetPropertiesOfSecrets(new List<SecretProperties>());

            List<SecretItem> result = await _management.GetKeys();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        #endregion

        #region GetDeletedKeys Tests

        [Test]
        public async Task GetDeletedKeys_ReturnsDeletedSecrets()
        {
            DeletedSecret mockDeletedSecret = CreateMockDeletedSecret("deleted1", new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
            List<DeletedSecret> deletedSecrets = new List<DeletedSecret> { mockDeletedSecret };

            SetupGetDeletedSecrets(deletedSecrets);

            List<SecretItem> result = await _management.GetDeletedKeys();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        #endregion

        #region GetKeyVersion Tests

        [Test]
        public async Task GetKeyVersion_ReturnsVersionsWithValues()
        {
            List<SecretProperties> versions = new List<SecretProperties>
            {
                new SecretProperties("testkey"),
                new SecretProperties("testkey")
            };

            SetupGetPropertiesOfSecretVersions("testkey", versions);

            SetupGetSecret("testkey", "v1", "value-v1");
            SetupGetSecret("testkey", "v2", "value-v2");

            SecretItem secretItem = new SecretItem { Name = "testkey" };
            List<SecretItem> result = await _management.GetKeyVersion(secretItem);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetKeyVersion_SingleVersion_ReturnsListWithoutFetchingValues()
        {
            List<SecretProperties> versions = new List<SecretProperties>
            {
                new SecretProperties("testkey")
            };

            SetupGetPropertiesOfSecretVersions("testkey", versions);

            SecretItem secretItem = new SecretItem { Name = "testkey" };
            List<SecretItem> result = await _management.GetKeyVersion(secretItem);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            _mockClient.Verify(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region SetExpiration Tests

        [Test]
        public async Task SetExpiration_ExistingKey_SetsExpiration()
        {
            KeyVaultSecret secret = new KeyVaultSecret("testkey", "testvalue");
            SetupGetSecret("testkey", null, secret);
            SetupSetSecret(secret);

            DateTime expirationDate = new DateTime(2025, 12, 31);
            await _management.SetExpiration("testkey", expirationDate);

            Assert.That(secret.Properties.ExpiresOn?.Date, Is.EqualTo(expirationDate.Date));
        }

        [Test]
        public async Task SetExpiration_NonExistingKey_DoesNotThrow()
        {
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(Response.FromValue<KeyVaultSecret>(null!, null)));
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(Response.FromValue<KeyVaultSecret>(null!, null)));

            Assert.DoesNotThrowAsync(async () => await _management.SetExpiration("nonexistent", DateTime.Now));
        }

        #endregion

        #region Helper Methods

        private DeletedSecret CreateMockDeletedSecret(string name, DateTimeOffset deletedOn)
        {
            SecretProperties properties = new SecretProperties(name);
            ConstructorInfo[] constructor = typeof(DeletedSecret).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            DeletedSecret deletedSecret = (DeletedSecret)constructor[0].Invoke(new object[] { properties });
            typeof(DeletedSecret).GetProperty("DeletedOn", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?.SetValue(deletedSecret, deletedOn);
            return deletedSecret;
        }

        private void SetupGetPropertiesOfSecrets(List<SecretProperties> secrets)
        {
            TestAsyncPageable<SecretProperties> asyncPageable = new TestAsyncPageable<SecretProperties>(secrets);
            _mockClient.Setup(c => c.GetPropertiesOfSecretsAsync(It.IsAny<CancellationToken>()))
                .Returns(asyncPageable);
        }

        private void SetupGetDeletedSecrets(List<DeletedSecret> secrets)
        {
            TestAsyncPageable<DeletedSecret> asyncPageable = new TestAsyncPageable<DeletedSecret>(secrets);
            _mockClient.Setup(c => c.GetDeletedSecretsAsync(It.IsAny<CancellationToken>()))
                .Returns(asyncPageable);
        }

        private void SetupGetPropertiesOfSecretVersions(string key, List<SecretProperties> versions)
        {
            TestAsyncPageable<SecretProperties> asyncPageable = new TestAsyncPageable<SecretProperties>(versions);
            _mockClient.Setup(c => c.GetPropertiesOfSecretVersionsAsync(key, It.IsAny<CancellationToken>()))
                .Returns(asyncPageable);
        }

        private void SetupGetSecret(string key, string expectedValue)
        {
            KeyVaultSecret secret = new KeyVaultSecret(key, expectedValue);
            Response<KeyVaultSecret> response = Response.FromValue(secret, null);
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(response));
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(response));
        }

        private void SetupGetSecret(string key, string version, string expectedValue)
        {
            KeyVaultSecret secret = new KeyVaultSecret(key, expectedValue);
            Response<KeyVaultSecret> response = Response.FromValue(secret, null);
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(response));
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(response));
        }

        private void SetupGetSecret(string key, string version, KeyVaultSecret secret)
        {
            Response<KeyVaultSecret> response = Response.FromValue(secret, null);
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(response));
            _mockClient.Setup(c => c.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(response));
        }

        private void SetupSetSecret(KeyVaultSecret secret)
        {
            Response<KeyVaultSecret> response = Response.FromValue(secret, null);
            _mockClient.Setup(c => c.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => Task.FromResult(response));
            _mockClient.Setup(c => c.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(response));
        }

        private class TestAsyncPageable<T> : AsyncPageable<T> where T : notnull
        {
            private readonly IEnumerable<T> _items;

            public TestAsyncPageable(IEnumerable<T> items)
            {
                _items = items;
            }

            public override async IAsyncEnumerable<Page<T>> AsPages(string continuationToken = null, int? pageSizeHint = null)
            {
                Page<T> page = Page<T>.FromValues(_items.ToList(), null, null!);
                yield return page;
                await Task.CompletedTask;
            }
        }

        #endregion
    }
}
