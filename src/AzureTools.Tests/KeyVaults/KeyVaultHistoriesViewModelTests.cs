using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.SDK.KeyVaults;
using AzureTools.Tests.Stubs;
using NUnit.Framework;
using System.Collections.Generic;

namespace AzureTools.Tests.KeyVaults
{
    public class KeyVaultHistoriesViewModelTests
    {
        private StubMessageCenter _messageCenter = null!;
        private StubExportFiles _exportFiles = null!;
        private KeyVaultSecretHistoriesViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _messageCenter = new StubMessageCenter();
            _exportFiles = new StubExportFiles();
            List<SecretItem> secrets = new List<SecretItem>
            {
                new() { Name = "test-key", Value = "v1" },
                new() { Name = "test-key", Value = "v2" }
            };
            _viewModel = new KeyVaultSecretHistoriesViewModel(new KeyVaultSecretHistoriesOption { Secrets = secrets, EnvironmentName = "TestEnv" }, _messageCenter, _exportFiles);
        }

        [Test]
        public void Constructor_SetsTitle()
        {
            Assert.That(_viewModel.Title, Does.Contain("TestEnv"));
            Assert.That(_viewModel.Title, Does.Contain("test-key"));
            Assert.That(_viewModel.Secrets.Count, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_EmptySecrets_UsesUnknownTitle()
        {
            KeyVaultSecretHistoriesViewModel vm = new KeyVaultSecretHistoriesViewModel(new KeyVaultSecretHistoriesOption { Secrets = new List<SecretItem>(), EnvironmentName = "Env1" }, _messageCenter, _exportFiles);

            Assert.That(vm.Title, Does.Contain("Unknown"));
        }

        [Test]
        public void Export_CallsSaveFileAndExport()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.SaveFileCalled, Is.True);
            Assert.That(_exportFiles.HistoriesToXlsxCalled, Is.True);
        }

        [Test]
        public void Export_UsesCorrectFileName()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.SaveFileDefaultName, Does.Contain("test-key"));
            Assert.That(_messageCenter.SaveFileDefaultName, Does.Contain("TestEnv"));
        }

        [Test]
        public void Export_SetsFilePathOnExportFiles()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_exportFiles.LastFilePath, Is.Not.Null);
            Assert.That(_exportFiles.LastFilePath, Does.EndWith(".xlsx"));
        }
    }
}
