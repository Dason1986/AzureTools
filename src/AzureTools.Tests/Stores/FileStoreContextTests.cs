using AzureTools.Stores;
using AzureTools.Tests.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Toolkit.Modules.Azure.Models;
using Toolkit.Modules.Models;

namespace AzureTools.Tests.Stores
{
    public class FileStoreContextTests
    {
        private string _testZipPath;
        private FileStoreContext _context;
        private IHostEnvironment _environment;

        [SetUp]
        public void SetUp()
        {
            _testZipPath = Path.Combine(Path.GetTempPath(), $"store_test_{Guid.NewGuid():N}.zip");
            ZipFileCreator.CreateEmptyZip(_testZipPath);

            _environment = new TestHostEnvironment(_testZipPath);
            IOptions<JsonSerializerOptions> jsonOptions = Options.Create(new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                TypeInfoResolver = AppJsonSerializerContext.Default
            });
            _context = new FileStoreContext(_environment, jsonOptions);
        }

        [TearDown]
        public void TearDown()
        {
            if (_context is IDisposable disposable)
                disposable.Dispose();

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    if (File.Exists(_testZipPath))
                    {
                        File.Delete(_testZipPath);
                    }
                    break;
                }
                catch (IOException)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        private FileStoreContext CreateContext()
        {
            IOptions<JsonSerializerOptions> jsonOptions = Options.Create(new JsonSerializerOptions()
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                TypeInfoResolver = AppJsonSerializerContext.Default
            });
            return new FileStoreContext(_environment, jsonOptions);
        }

        #region GetSet Tests

        [Test]
        public void GetSet_EmptyZip_ReturnsEmptyCollection()
        {
            IStoreSet<AppProfile> result = _context.GetSet<AppProfile>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void GetSet_AfterSave_ReturnsSavedData()
        {
            AppProfile profile = new AppProfile { Name = "Test Profile" };
            _context.Add(profile);
            _context.SaveChanges();

            using FileStoreContext newContext = CreateContext();
            System.Collections.Generic.List<AppProfile> result = newContext.GetSet<AppProfile>().ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Test Profile"));
        }

        #endregion

        #region Find Tests

        [Test]
        public void Find_EmptyCollection_ReturnsNull()
        {
            AppProfile result = _context.Find<AppProfile>();
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Find_WithData_ReturnsFirstItem()
        {
            AppProfile profile = new AppProfile { Name = "Test" };
            _context.Add(profile);

            AppProfile result = _context.Find<AppProfile>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Test"));
        }

        #endregion

        #region Add Tests

        [Test]
        public void Add_SingleItem_IncreasesCount()
        {
            _context.Add(new AppProfile { Name = "Test1" });
            _context.Add(new AppProfile { Name = "Test2" });

            System.Collections.Generic.List<AppProfile> result = _context.GetSet<AppProfile>().ToList();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void Add_NullItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.Add<AppProfile>(null!));
        }

        [Test]
        public void AddRange_MultipleItems_IncreasesCount()
        {
            KeyVaultEnv[] items = new[]
            {
                new KeyVaultEnv { Name = "Env1", KeyVault = "kv1" },
                new KeyVaultEnv { Name = "Env2", KeyVault = "kv2" },
                new KeyVaultEnv { Name = "Env3", KeyVault = "kv3" }
            };

            _context.AddRange(items);

            System.Collections.Generic.List<KeyVaultEnv> result = _context.GetSet<KeyVaultEnv>().ToList();
            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void AddRange_NullItems_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.AddRange<AppProfile>(null!));
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_ExistingItem_UpdatesData()
        {
            SettingOptioinItem profile = new SettingOptioinItem { SettingValue = "Original" };
            _context.Add(profile);

            profile.SettingValue = "Updated";
            _context.Update(profile);
            _context.SaveChanges();
            SettingOptioinItem result = _context.Find<SettingOptioinItem>();
            Assert.That(result!.SettingValue, Is.EqualTo("Updated"));
        }

        [Test]
        public void Update_NonExistingItem_AddsItem()
        {
            SettingOptioinItem profile = new SettingOptioinItem { SettingValue = "New" };
            _context.Update(profile);

            System.Collections.Generic.List<SettingOptioinItem> result = _context.GetSet<SettingOptioinItem>().ToList();
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].SettingValue, Is.EqualTo("New"));
        }

        [Test]
        public void Update_NullItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.Update<AppProfile>(null!));
        }

        #endregion

        #region Remove Tests

        [Test]
        public void Remove_ExistingItem_DecreasesCount()
        {
            AppProfile profile = new AppProfile { Name = "ToRemove" };
            _context.Add(profile);
            _context.SaveChanges();
            _context.Remove(profile);
            _context.SaveChanges();

            System.Collections.Generic.List<AppProfile> result = _context.GetSet<AppProfile>().ToList();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void Remove_NonExistingItem_DoesNotThrow()
        {
            AppProfile profile = new AppProfile { Name = "NotExist" };
            Assert.DoesNotThrow(() => _context.Remove(profile));
        }

        [Test]
        public void Remove_NullItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.Remove<AppProfile>(null!));
        }

        [Test]
        public void RemoveRange_MultipleItems_RemovesAll()
        {
            KeyVaultEnv item1 = new KeyVaultEnv { Name = "Item1" };
            KeyVaultEnv item2 = new KeyVaultEnv { Name = "Item2" };
            _context.Add(item1);
            _context.Add(item2);

            _context.RemoveRange(new[] { item1, item2 });

            System.Collections.Generic.List<KeyVaultEnv> result = _context.GetSet<KeyVaultEnv>().ToList();
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveRange_NullItems_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _context.RemoveRange<AppProfile>(null!));
        }

        #endregion

        #region SaveChanges Tests

        [Test]
        public void SaveChanges_DataPersistsAcrossSessions()
        {
            AppProfile profile = new AppProfile { Name = "Persistent" };
            _context.Add(profile);
            _context.SaveChanges();

            using FileStoreContext newContext = CreateContext();
            AppProfile result = newContext.Find<AppProfile>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Persistent"));
        }

        [Test]
        public void SaveChanges_UpdatePersistsAcrossSessions()
        {
            SettingOptioinItem profile = new SettingOptioinItem { SettingValue = "Original" };
            _context.Add(profile);
            _context.SaveChanges();

            profile.SettingValue = "Updated";
            _context.Update(profile);
            _context.SaveChanges();

            using FileStoreContext newContext = CreateContext();
            SettingOptioinItem result = newContext.Find<SettingOptioinItem>();

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.SettingValue, Is.EqualTo("Updated"));
        }

        [Test]
        public void SaveChanges_RemovePersistsAcrossSessions()
        {
            SettingOptioinItem profile = new SettingOptioinItem { SettingValue = "ToRemove" };
            _context.Add(profile);
            _context.SaveChanges();

            _context.Remove(profile);
            _context.SaveChanges();

            using FileStoreContext newContext = CreateContext();
            SettingOptioinItem result = newContext.Find<SettingOptioinItem>();

            Assert.That(result, Is.Null);
        }

        [Test]
        public void SaveChanges_MultipleTypesPersist()
        {
            _context.Add(new SettingOptioinItem { SettingValue = "Profile1" });
            _context.Add(new KeyVaultEnv { Name = "Env1", KeyVault = "kv1" });
            _context.SaveChanges();

            using FileStoreContext newContext = CreateContext();

            System.Collections.Generic.List<SettingOptioinItem> profiles = newContext.GetSet<SettingOptioinItem>().ToList();
            System.Collections.Generic.List<KeyVaultEnv> envs = newContext.GetSet<KeyVaultEnv>().ToList();

            Assert.That(profiles.Count, Is.EqualTo(1));
            Assert.That(profiles[0].SettingValue, Is.EqualTo("Profile1"));
            Assert.That(envs.Count, Is.EqualTo(1));
            Assert.That(envs[0].Name, Is.EqualTo("Env1"));
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullCrudWorkflow_AllOperationsWork()
        {
            SettingOptioinItem profile = new SettingOptioinItem
            {
                SettingValue = "Integration Test",
                SettingGroup = TenantMode.Single.ToString()
            };

            _context.Add(profile);
            _context.SaveChanges();

            using FileStoreContext verifyContext = CreateContext();
            SettingOptioinItem saved = verifyContext.Find<SettingOptioinItem>();
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved!.SettingValue, Is.EqualTo("Integration Test"));

            saved.SettingValue = "Updated Name";
            verifyContext.Update(saved);
            verifyContext.SaveChanges();

            using FileStoreContext updateContext = CreateContext();
            SettingOptioinItem updated = updateContext.Find<SettingOptioinItem>();
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.SettingValue, Is.EqualTo("Updated Name"));

            updateContext.Remove(updated);
            updateContext.SaveChanges();

            using FileStoreContext finalContext = CreateContext();
            SettingOptioinItem result = finalContext.Find<SettingOptioinItem>();
            Assert.That(result, Is.Null);
        }

        #endregion

        #region EnableEncryption Tests

        [Test]
        public void EnableEncryption_EncryptsExistingData()
        {
            _context.Add(new AppProfile { Name = "Test Profile" });
            _context.Add(new SettingOptioinItem { SettingValue = "Setting1" });
            _context.SaveChanges();

            Assert.That(_context.Encrypted, Is.False);

            _context.EnableEncryption();

            Assert.That(_context.Encrypted, Is.True);

            using FileStoreContext verifyContext = CreateContext();
            System.Collections.Generic.List<AppProfile> profiles = verifyContext.GetSet<AppProfile>().ToList();
            System.Collections.Generic.List<SettingOptioinItem> settings = verifyContext.GetSet<SettingOptioinItem>().ToList();

            Assert.That(profiles.Count, Is.EqualTo(1));
            Assert.That(profiles[0].Name, Is.EqualTo("Test Profile"));
            Assert.That(settings.Count, Is.EqualTo(1));
            Assert.That(settings[0].SettingValue, Is.EqualTo("Setting1"));
        }

        [Test]
        public void EnableEncryption_AlreadyEncrypted_DoesNotThrow()
        {
            _context.EnableEncryption();
            Assert.That(_context.Encrypted, Is.True);

            Assert.DoesNotThrow(() => _context.EnableEncryption());
        }

        [Test]
        public void EnableEncryption_NewDataAfterEncryption_IsEncrypted()
        {
            _context.EnableEncryption();
            _context.Add(new AppProfile { Name = "After Encrypt" });
            _context.SaveChanges();

            using FileStoreContext verifyContext = CreateContext();
            System.Collections.Generic.List<AppProfile> profiles = verifyContext.GetSet<AppProfile>().ToList();

            Assert.That(profiles.Count, Is.EqualTo(1));
            Assert.That(profiles[0].Name, Is.EqualTo("After Encrypt"));
        }

        #endregion

        #region Helper Classes

        private class TestHostEnvironment : IHostEnvironment
        {
            public TestHostEnvironment(string zipPath)
            {
                ApplicationName = "Test";
                EnvironmentName = "Test";
                ContentRootPath = Path.GetDirectoryName(zipPath);
                ContentRootFileProvider = new TestFileProvider(zipPath);
            }

            public string ApplicationName { get; set; }
            public string EnvironmentName { get; set; }
            public string ContentRootPath { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }

        private class TestFileProvider : IFileProvider
        {
            private readonly string _zipPath;

            public TestFileProvider(string zipPath)
            {
                _zipPath = zipPath;
            }

            public IFileInfo GetFileInfo(string subpath)
            {
                return new TestFileInfo(_zipPath);
            }

            public IDirectoryContents GetDirectoryContents(string subpath)
            {
                throw new NotImplementedException();
            }

            public Microsoft.Extensions.Primitives.IChangeToken Watch(string filter)
            {
                throw new NotImplementedException();
            }
        }

        private class TestFileInfo : IFileInfo
        {
            public TestFileInfo(string physicalPath)
            {
                PhysicalPath = physicalPath;
            }

            public bool Exists => true;
            public long Length => 0;
            public string PhysicalPath { get; }
            public string Name => Path.GetFileName(PhysicalPath);
            public DateTimeOffset LastModified => DateTimeOffset.Now;
            public bool IsDirectory => false;
            public Stream CreateReadStream() => File.OpenRead(PhysicalPath);
        }

        #endregion
    }
}
