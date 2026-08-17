using AzureTools.Tools.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureTools.Tests.IO
{
    public class ZipFileProviderBuilderTests
    {
        private string _testZipPath;

        [SetUp]
        public void SetUp()
        {
            _testZipPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.zip");
        }

        [TearDown]
        public void TearDown()
        {
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

        [Test]
        public void Create_WithValidPath_ReturnsBuilder()
        {
            ZipFileProviderBuilder builder = ZipFileProviderBuilder.Create(_testZipPath);
            Assert.That(builder, Is.Not.Null);
        }

        [Test]
        public void Build_WithNonExistentFile_CreatesNewZipFile()
        {
            ZipFileProviderBuilder builder = ZipFileProviderBuilder.Create(_testZipPath);
            using IStoreFileProvider provider = builder.Build();
            Assert.That(provider, Is.Not.Null);
            Assert.That(File.Exists(_testZipPath), Is.True);
        }

        [Test]
        public void Build_WithEmptyPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ZipFileProviderBuilder.Create("").Build());
        }

        [Test]
        public void Build_WithNullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ZipFileProviderBuilder.Create(null!).Build());
        }

        [Test]
        public void WriteFile_AndGetContent_ReturnsSameContent()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            provider.WriteFile("test.json", Encoding.UTF8.GetBytes("{\"name\":\"test\"}"));
            byte[] content = provider.GetBytes("test.json");

            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("{\"name\":\"test\"}"));
        }

        [Test]
        public void GetContent_WithNonExistentFile_ReturnsEmptyString()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            byte[] content = provider.GetBytes("nonexistent.json");

            Assert.That(content.Length, Is.EqualTo(0));
        }

        [Test]
        public void WriteFile_UpdateExistingFile_OverwritesContent()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            provider.WriteFile("test.json", Encoding.UTF8.GetBytes("original"));
            provider.WriteFile("test.json", Encoding.UTF8.GetBytes("updated"));
            byte[] content = provider.GetBytes("test.json");

            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("updated"));
        }

        [Test]
        public void DeleteFile_ExistingFile_RemovesFile()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            provider.WriteFile("test.json", Encoding.UTF8.GetBytes("content"));
            provider.DeleteFile("test.json");
            byte[] content = provider.GetBytes("test.json");

            Assert.That(content.Length, Is.EqualTo(0));
        }

        [Test]
        public void DeleteFile_NonExistentFile_DoesNotThrow()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            Assert.DoesNotThrow(() => provider.DeleteFile("nonexistent.json"));
        }

        [Test]
        public void GetFileNames_MultipleFiles_ReturnsAllFileNames()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            provider.WriteFile("file1.json", Encoding.UTF8.GetBytes("content1"));
            provider.WriteFile("file2.json", Encoding.UTF8.GetBytes("content2"));
            provider.WriteFile("file3.json", Encoding.UTF8.GetBytes("content3"));

            List<string> names = provider.GetFileNames().ToList();

            Assert.That(names.Count, Is.EqualTo(3));
            Assert.That(names, Does.Contain("file1.json"));
            Assert.That(names, Does.Contain("file2.json"));
            Assert.That(names, Does.Contain("file3.json"));
        }

        [Test]
        public void GetFileNames_EmptyZip_ReturnsEmptyList()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            List<string> names = provider.GetFileNames().ToList();

            Assert.That(names.Count, Is.EqualTo(0));
        }

        [Test]
        public void WriteFile_WithSpecialCharacters_HandlesCorrectly()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            string specialContent = "{\"name\":\"测试\",\"value\":\"特殊字符: <>&\"}";
            provider.WriteFile("special.json", Encoding.UTF8.GetBytes(specialContent));
            byte[] content = provider.GetBytes("special.json");

            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo(specialContent));
        }

        [Test]
        public void WriteFile_WithPassword_RequiresPasswordToRead()
        {
            ZipFileCreator.CreateEncryptedZip(_testZipPath, "testpass");
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).WithPassword("testpass").Build();

            provider.WriteFile("secret.json", Encoding.UTF8.GetBytes("encrypted content"));
            byte[] content = provider.GetBytes("secret.json");

            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("encrypted content"));
        }

        [Test]
        public void WriteFile_PersistsAcrossSessions()
        {
            using (IStoreFileProvider provider1 = ZipFileProviderBuilder.Create(_testZipPath).Build())
            {
                provider1.WriteFile("persistent.json", Encoding.UTF8.GetBytes("persisted data"));
            }

            using IStoreFileProvider provider2 = ZipFileProviderBuilder.Create(_testZipPath).Build();
            byte[] content = provider2.GetBytes("persistent.json");

            Assert.That(Encoding.UTF8.GetString(content), Is.EqualTo("persisted data"));
        }

        [Test]
        public void GetFileNames_AfterDelete_ExcludesDeletedFile()
        {
            using IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).Build();

            provider.WriteFile("keep.json", Encoding.UTF8.GetBytes("keep"));
            provider.WriteFile("remove.json", Encoding.UTF8.GetBytes("remove"));
            provider.DeleteFile("remove.json");

            List<string> names = provider.GetFileNames().ToList();

            Assert.That(names.Count, Is.EqualTo(1));
            Assert.That(names, Does.Contain("keep.json"));
            Assert.That(names, Does.Not.Contain("remove.json"));
        }

        [Test]
        public void Build_WithPassword_CreatesEncryptedZip()
        {
            using (IStoreFileProvider provider = ZipFileProviderBuilder.Create(_testZipPath).WithPassword("testpass").Build())
            {
                provider.WriteFile("test.json", Encoding.UTF8.GetBytes("encrypted content"));
            }

            Assert.That(File.Exists(_testZipPath), Is.True);
        }
    }
}