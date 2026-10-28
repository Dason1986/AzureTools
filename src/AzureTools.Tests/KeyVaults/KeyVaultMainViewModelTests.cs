using AzureTools.SDK;
using AzureTools.SDK.Models;
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
using Toolkit.Modules.Azure.Properties;
using Toolkit.Modules.Azure.Stores;
using Toolkit.Modules.Azure.ViewModels.KeyVaults;
using Toolkit.Modules.Models;
using Toolkit.Modules.Stores;

namespace AzureTools.Tests.KeyVaults
{
    public class KeyVaultMainViewModelTests
    {
        private Mock<IKeyVaultCompareService> _mockCompareService = null!;
        private Mock<IToolkitStore> _mockAzureToolsStore = null!;
        private Mock<IAzureResourceFactory> _mockResourceFactory = null!;
        private StubMessageCenter _messageCenter = null!;
        private List<KeyVaultEnv> _keyVaultOptions = null!;
        private KeyVaultSecretMainViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _mockCompareService = StubKeyVaultCompareService.Create();
            _mockAzureToolsStore = new Mock<IToolkitStore>();
            _mockResourceFactory = new Mock<IAzureResourceFactory>();
            _messageCenter = new StubMessageCenter();
            _keyVaultOptions = new List<KeyVaultEnv>
            {
                new() { Name = "Env1", KeyVault = "env1-vault",  },
                new() { Name = "Env2", KeyVault = "env2-vault" },
                new() { Name = "Env3", KeyVault = "env3-vault" }
            };
            _mockAzureToolsStore.Setup(s => s.GetTenant(It.IsAny<string>())).Returns(new TenantProfile());
            _mockAzureToolsStore.Setup(s => s.GetProjects()).Returns(new ProjectInfo[] { new() { Id = Guid.NewGuid(), Name = "TestProject" } });
            _viewModel = new KeyVaultSecretMainViewModel(
                _mockCompareService.Object,
                _mockAzureToolsStore.Object,
                _mockResourceFactory.Object,
                CreateStore(_keyVaultOptions),
                _messageCenter,
                NullLogger<KeyVaultSecretMainViewModel>.Instance);
        }

        private static IKeyVaultStore CreateStore(List<KeyVaultEnv> options)
        {
            Mock<IKeyVaultStore> mock = new Mock<IKeyVaultStore>();
            mock.Setup(s => s.GetOptions()).Returns(options);
            return mock.Object;
        }

        [Test]
        public void Constructor_WithValidOptions_SetsProperties()
        {
            Assert.That(_viewModel.KeyVaultOptionsList.Count, Is.EqualTo(3));
            Assert.That(_viewModel.SelectedKeyVaultOption, Is.Not.Null);
            Assert.That(_viewModel.SelectedKeyVaultOption.Name, Is.EqualTo("Env1"));
            Assert.That(_viewModel.IsCompareEnabled, Is.True);
        }

        [Test]
        public void Constructor_WithSingleOption_DisablesCompare()
        {
            List<KeyVaultEnv> options = new List<KeyVaultEnv>
            {
                new() { Name = "Env1", KeyVault = "env1-vault" }
            };
            Mock<IToolkitStore> mockStore = new Mock<IToolkitStore>();
            mockStore.Setup(s => s.GetTenant(It.IsAny<string>())).Returns(new TenantProfile());
            mockStore.Setup(s => s.GetProjects()).Returns(new ProjectInfo[] { new() { Id = Guid.NewGuid(), Name = "TestProject" } });
            KeyVaultSecretMainViewModel vm = new KeyVaultSecretMainViewModel(
                _mockCompareService.Object,
                mockStore.Object,
                _mockResourceFactory.Object,
                CreateStore(options),
                _messageCenter,
                NullLogger<KeyVaultSecretMainViewModel>.Instance);

            Assert.That(vm.IsCompareEnabled, Is.False);
        }

        [Test]
        public void EnterKeyVault_NoSelection_ShowsMessage()
        {
            _viewModel.SelectedProject = _viewModel.ProjectList.FirstOrDefault();
            _viewModel.SelectedKeyVaultOption = null;

            _viewModel.EnterKeyVaultCommand.Execute(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(AzureResource.SelectValidEnv));
        }

        [Test]
        public void EnterKeyVault_InvalidKeyVault_ShowsMessage()
        {
            _viewModel.SelectedProject = _viewModel.ProjectList.FirstOrDefault();
            _viewModel.SelectedKeyVaultOption = new KeyVaultEnv { Name = "Test", KeyVault = "" };

            _viewModel.EnterKeyVaultCommand.Execute(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(AzureResource.InvalidEnvConfig));
        }

        [Test]
        public async Task CompareAsync_LessThanTwoSelected_ShowsMessage()
        {
            _viewModel.SelectedCompareOptions = new List<KeyVaultEnv> { _keyVaultOptions[0] };

            await _viewModel.CompareCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastInfoMessage, Is.EqualTo(AzureResource.SelectTwoEnvs));
        }

        [Test]
        public async Task CompareAsync_ValidComparison_CallsService()
        {
            _viewModel.SelectedCompareOptions = new List<KeyVaultEnv> { _keyVaultOptions[0], _keyVaultOptions[1] };

            await _viewModel.CompareCommand.ExecuteAsync(null);

            _mockCompareService.Verify(
                m => m.CompareAsync(It.IsAny<KeyVaultAzureEnv[]>()),
                Times.Once);
        }

        [Test]
        public async Task CompareAsync_ServiceThrows_ShowsError()
        {
            _mockCompareService.Setup(m => m.CompareAsync(It.IsAny<KeyVaultAzureEnv[]>()))
                .ThrowsAsync(new InvalidOperationException("compare failed"));
            _viewModel.SelectedCompareOptions = new List<KeyVaultEnv> { _keyVaultOptions[0], _keyVaultOptions[1] };

            await _viewModel.CompareCommand.ExecuteAsync(null);

            Assert.That(_messageCenter.LastErrorMessage, Does.Contain("compare failed"));
        }
    }
}
