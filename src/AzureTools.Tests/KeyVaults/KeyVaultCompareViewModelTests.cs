using AzureTools.Module.Azure.ViewModels.KeyVaults;
using AzureTools.Services.KeyVaults;
using AzureTools.Tests.Stubs;
using NUnit.Framework;
using System.Collections.Generic;

namespace AzureTools.Tests.KeyVaults
{
    public class KeyVaultCompareViewModelTests
    {
        private StubMessageCenter _messageCenter = null!;
        private StubExportFiles _exportFiles = null!;
        private KeyVaultSecretCompareViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _messageCenter = new StubMessageCenter();
            _exportFiles = new StubExportFiles();
            List<SecretCompareReport> results = new List<SecretCompareReport>
            {
                new() { KeyName = "key1", Status = CompareStatus.AllExist, EnvStatusMap = { ["Env1"] = "存在", ["Env2"] = "存在" }, EnvValueMap = { ["Env1"] = "val1", ["Env2"] = "val1" } },
                new() { KeyName = "key2", Status = CompareStatus.Partial, EnvStatusMap = { ["Env1"] = "存在", ["Env2"] = "缺失" } },
                new() { KeyName = "key3", Status = CompareStatus.Partial, EnvStatusMap = { ["Env1"] = "缺失", ["Env2"] = "存在" } },
                new() { KeyName = "key4", Status = CompareStatus.Partial, EnvStatusMap = { ["Env1"] = "缺失", ["Env2"] = "缺失" } }
            };
            _viewModel = new KeyVaultSecretCompareViewModel(new KeyVaultSecretCompareOption { EnvNames = new List<string> { "Env1", "Env2" }, Results = results }, _messageCenter, _exportFiles);
        }

        [Test]
        public void Constructor_SetsProperties()
        {
            Assert.That(_viewModel.EnvNames.Count, Is.EqualTo(2));
            Assert.That(_viewModel.Results.Count, Is.EqualTo(4));
        }

        [Test]
        public void Constructor_GeneratesSummaryText()
        {
            Assert.That(_viewModel.SummaryText, Does.Contain("共 4 条记录"));
            Assert.That(_viewModel.SummaryText, Does.Contain("全部存在: 1"));
            Assert.That(_viewModel.SummaryText, Does.Contain("部分存在: 3"));
        }

        [Test]
        public void Export_CallsSaveFileAndExport()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.SaveFileCalled, Is.True);
            Assert.That(_exportFiles.CompareToXlsxCalled, Is.True);
        }

        [Test]
        public void Export_UsesCorrectFileName()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.SaveFileDefaultName, Does.Contain("Env1"));
            Assert.That(_messageCenter.SaveFileDefaultName, Does.Contain("Env2"));
        }

        [Test]
        public void Export_OpensFileWithExplorer()
        {
            _viewModel.ExportCommand.Execute(null);

            Assert.That(_messageCenter.LastOpenedPath, Is.Not.Null);
        }

        [Test]
        public void UpdateSummary_NoMissing_ShowsZeroCounts()
        {
            List<SecretCompareReport> results = new List<SecretCompareReport>
            {
                new() { KeyName = "key1", Status = CompareStatus.AllExist }
            };
            KeyVaultSecretCompareViewModel vm = new KeyVaultSecretCompareViewModel(new KeyVaultSecretCompareOption { EnvNames = new List<string> { "Env1", "Env2" }, Results = results }, _messageCenter, _exportFiles);

            Assert.That(vm.SummaryText, Does.Contain("共 1 条记录"));
            Assert.That(vm.SummaryText, Does.Contain("全部存在: 1"));
            Assert.That(vm.SummaryText, Does.Contain("部分存在: 0"));
        }
    }
}
