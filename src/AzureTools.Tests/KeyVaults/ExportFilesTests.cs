using AzureTools.SDK.KeyVaults;
using AzureTools.Services.KeyVaults;
using AzureTools.Tests.Stubs;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace AzureTools.Tests.KeyVaults
{
    public class ExportFilesTests
    {
        private StubMessageCenter _messageCenter = null!;
        private KeyVaultExportFiles _exportFiles = null!;

        [SetUp]
        public void SetUp()
        {
            _messageCenter = new StubMessageCenter();
            _exportFiles = new KeyVaultExportFiles(_messageCenter);
        }

        [Test]
        public void Constructor_WithMessageCenter_CreatesInstance()
        {
            Assert.That(_exportFiles, Is.Not.Null);
        }

        [Test]
        public void CompareToXlsx_WithData_CallsOpenWithExplorer()
        {
            List<SecretCompareReport> results = new List<SecretCompareReport>
            {
                new() { KeyName = "test-key", EnvStatusMap = { ["Env1"] = "OK", ["Env2"] = "OK" }, EnvValueMap = { ["Env1"] = "val1", ["Env2"] = "val2" } }
            };

            using MemoryStream stream = new MemoryStream();
            _exportFiles.CompareToXlsx(results, stream, "compare.xlsx", new List<string> { "Env1", "Env2" });

            Assert.That(_messageCenter.LastOpenedPath, Is.EqualTo("compare.xlsx"));
        }

        [Test]
        public void HistoriesToXlsx_WithData_CallsOpenWithExplorer()
        {
            List<SecretItem> items = new List<SecretItem>
            {
                new() { Name = "history-key", Value = "history-value" }
            };

            using MemoryStream stream = new MemoryStream();
            _exportFiles.HistoriesToXlsx(items, stream, "history.xlsx");

            Assert.That(_messageCenter.LastOpenedPath, Is.EqualTo("history.xlsx"));
        }

        [Test]
        public void ToXlsx_EmptyData_CallsOpenWithExplorer()
        {
            using MemoryStream stream = new MemoryStream();
            _exportFiles.ToXlsx(new List<SecretItem>(), stream, "empty.xlsx");

            Assert.That(_messageCenter.LastOpenedPath, Is.EqualTo("empty.xlsx"));
        }
    }
}
