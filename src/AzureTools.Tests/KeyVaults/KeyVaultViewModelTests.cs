using AzureTools.SDK.KeyVaults;
using AzureTools.Tests.Stubs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolkit.Modules.Azure.KeyVaults;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Azure.Models.KeyVaults;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;
using Toolkit.Modules.Models;
using Toolkit.Modules.Properties;

namespace AzureTools.Tests.KeyVaults
{
    public class KeyVaultViewModelTests
    {
        private Mock<IKeyVaultManagement> _mockKeyVault = null!;
        private Mock<IKeyVaultStore> _mockKeyVaultStore = null!;
        private StubMessageCenter _messageCenter = null!;
        private StubExportFiles _exportFiles = null!;
        private KeyVaultSecretViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mockKeyVault = StubKeyVaultManagement.Create("testvault");
            _mockKeyVaultStore = SubKeyVaultStore.Create();
            _messageCenter = new StubMessageCenter();
            _exportFiles = new StubExportFiles();
            _viewModel = new KeyVaultSecretViewModel(new KeyVaultEnv() { Name = "TestEnv", KeyVault = "" }, new ProjectInfo { Id = Guid.NewGuid(), Name = "TestProject" }, _mockKeyVault.Object, _mockKeyVaultStore.Object, _messageCenter, _exportFiles, NullLogger<KeyVaultSecretViewModel>.Instance);
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            Assert.That(_viewModel.EnvironmentName, Is.EqualTo("TestEnv"));
            Assert.That(_viewModel.WindowTitle, Is.EqualTo("KeyVault 管理 - TestEnv"));
            Assert.That(_viewModel.KeyVaults, Is.Not.Null);
            Assert.That(_viewModel.KeyVaults.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllAsync_PopulatesKeyVaults()
        {
            List<SecretItem> keys = new List<SecretItem>
            {
                new() { Name = "key1",   Value = "val1" },
                new() { Name = "key2",  Value = "val2" }
            };
            _mockKeyVault.Setup(m => m.GetKeys()).ReturnsAsync(keys);
            _mockKeyVault.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string k, string v) => keys.FirstOrDefault(x => x.Name == k)?.Value);

            await _viewModel.GetAllCommand.ExecuteAsync(null);

            Assert.That(_viewModel.KeyVaults.Count, Is.EqualTo(2));
            Assert.That(_viewModel.SelectedSecretItem, Is.Not.Null);
        }

        [Test]
        public async Task UpdateAsync_NoSelection_ShowsMessage()
        {
            _viewModel.SelectedSecretItem = null;
            await _viewModel.UpdateCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoRecordSelected));
            _mockKeyVault.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ValueUnchanged_ShowsMessage()
        {
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "key1", Value = "same" };
            _viewModel.CurrentValue = "same";

            await _viewModel.UpdateCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoModification));
            _mockKeyVault.Verify(m => m.Set(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task UpdateAsync_ValidInput_CallsSet()
        {
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "KEY-1", Value = "old" };
            _viewModel.CurrentValue = "new";

            await _viewModel.UpdateCommand.ExecuteAsync(null);

            _mockKeyVault.Verify(m => m.Set("KEY-1", "new"), Times.Once);
            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.UpdateSuccess));
        }

        [Test]
        public async Task UpdateAsync_SetThrows_ShowsError()
        {
            _mockKeyVault.Setup(m => m.Set(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("test error"));
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "KEY-1", Value = "old" };
            _viewModel.CurrentValue = "new";

            await _viewModel.UpdateCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastErrorMessage, Is.EqualTo("test error"));
        }

        [Test]
        public async Task HistoryAsync_NoSelection_ShowsMessage()
        {
            _viewModel.SelectedSecretItem = null;
            await _viewModel.HistoryCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoRecordSelected));
        }

        [Test]
        public async Task HistoryAsync_SingleVersion_ShowsMessage()
        {
            SecretItemModel item = new SecretItemModel { Name = "key1" };
            _viewModel.SelectedSecretItem = item;
            _mockKeyVault.Setup(m => m.GetKeyVersion(It.IsAny<SecretItem>()))
                .ReturnsAsync(new List<SecretItem> { item });

            await _viewModel.HistoryCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoMultipleVersions));
        }

        [Test]
        public async Task SetAllExpirationAsync_NoKeys_DoesNothing()
        {
            await _viewModel.SetAllExpirationCommand.ExecuteAsync(null);

            _mockKeyVault.Verify(m => m.SetExpiration(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task SetAllExpirationAsync_WithKeys_SetsExpiration()
        {
            _viewModel.KeyVaults = new System.Collections.ObjectModel.ObservableCollection<SecretItemModel>
            {
                new() { Name = "key1", Created = DateTimeOffset.Now },
                new() { Name = "key2", Created = DateTimeOffset.Now }
            };
            _viewModel.ExpirationDate = new DateTime(2025, 12, 31);

            await _viewModel.SetAllExpirationCommand.ExecuteAsync(null);

            _mockKeyVault.Verify(m => m.SetExpiration(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(2));
            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.BatchSetExpirationSuccess));
        }

        [Test]
        public async Task SetOneExpirationAsync_NoSelection_ShowsMessage()
        {
            _viewModel.SelectedSecretItem = null;
            await _viewModel.SetOneExpirationCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoRecordSelected));
        }

        [Test]
        public async Task SetOneExpirationAsync_NotInVault_ShowsMessage()
        {
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "key1" };

            await _viewModel.SetOneExpirationCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.RecordNotInKeyVault));
        }

        [Test]
        public async Task SetOneExpirationAsync_Valid_SetsExpiration()
        {
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "key1", Created = DateTimeOffset.Now };
            _viewModel.ExpirationDate = new DateTime(2025, 12, 31);

            await _viewModel.SetOneExpirationCommand.ExecuteAsync(null);

            _mockKeyVault.Verify(m => m.SetExpiration("key1", It.IsAny<DateTime>()), Times.Once);
            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.UpdateSuccess));
        }

        [Test]
        public void Export_NoKeys_ShowsMessage()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.GetConfigDataFirst));
            Assert.That(_exportFiles.ToXlsxCalled, Is.False);
        }

        [Test]
        public void Export_WithKeys_CallsExport()
        {
            _viewModel.KeyVaults = new System.Collections.ObjectModel.ObservableCollection<SecretItemModel>
            {
                new() { Name = "key1", Value = "val1" }
            };

            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.SaveFileCalled, Is.True);
            Assert.That(_exportFiles.ToXlsxCalled, Is.True);
        }

        [Test]
        public async Task ValidatorAsync_EmptyKeys_CallsGetAll()
        {
            _mockKeyVault.Setup(m => m.GetKeys())
                .ReturnsAsync(new List<SecretItem>());

            await _viewModel.ValidatorCommand.ExecuteAsync(null);

            _mockKeyVault.Verify(m => m.GetKeys(), Times.Once);
        }

        [Test]
        public async Task ValidatorAsync_NoMissingKeys_ShowsSuccess()
        {
            _mockKeyVaultStore.Setup(m => m.GetRequiredKeys(It.IsAny<Guid>()))
                .Returns(Array.Empty<SecretKeyOptionItem>());
            _viewModel.KeyVaults = new System.Collections.ObjectModel.ObservableCollection<SecretItemModel>();

            await _viewModel.ValidatorCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(MessageResources.NoMissingConfig));
        }

        [Test]
        public void OnSelectedSecretItemChanged_UpdatesCurrentValue()
        {
            SecretItemModel item = new SecretItemModel { Name = "key1", Value = "test-value" };

            _viewModel.SelectedSecretItem = item;

            Assert.That(_viewModel.CurrentValue, Is.EqualTo("test-value"));
        }

        [Test]
        public void OnSelectedSecretItemChanged_NullItem_ClearsValue()
        {
            _viewModel.SelectedSecretItem = new SecretItemModel { Name = "key1", Value = "test" };
            _viewModel.CurrentValue = "some-value";

            _viewModel.SelectedSecretItem = null;

            Assert.That(_viewModel.CurrentValue, Is.EqualTo(string.Empty));
        }
    }
}
