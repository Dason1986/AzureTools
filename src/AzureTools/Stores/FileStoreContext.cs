using AzureTools.Tools.Encryptions;
using AzureTools.Tools.IO;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace AzureTools.Stores
{

    public class FileStoreContext : IStoreContext, IDisposable
    {
        #region Constructors

        public FileStoreContext(Microsoft.Extensions.Hosting.IHostEnvironment environment, IOptions<JsonSerializerOptions> jsonOptions)
        {
            string zipPath = environment.ContentRootFileProvider.GetFileInfo("appdata.zip").PhysicalPath;
            _fileProvider = ZipFileProviderBuilder.Create(zipPath).Build();
            _jsonOptions = jsonOptions.Value;


            _encryptionService = new AesEncryptionService(_fileProvider);

            Encrypted = _encryptionService.LoadPackage();

        }

        #endregion

        #region Fields & Properties

        private bool _disposed;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IStoreFileProvider _fileProvider;
        private readonly Dictionary<Type, IListSource> _cache = new();
        private IEncryptionService _encryptionService;

        public bool Encrypted { get; private set; }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _fileProvider?.Dispose();
        }
        string GetDocumentsName(string name)
        {
            return $"/documents/{name}.json";
        }


        public TEntity Find<TEntity>() where TEntity : class
        {
            return GetSet<TEntity>().FirstOrDefault();
        }

        public void Add<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            GetSet<TEntity>().Add(item);
        }

        public void AddRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            GetSet<TEntity>().AddRange(items);
        }

        public void Update<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            GetSet<TEntity>().Update(item);
        }

        public void Remove<TEntity>(TEntity item) where TEntity : class
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            GetSet<TEntity>().Remove(item);
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            GetSet<TEntity>().RemoveRange(items);

        }
        public IStoreSet<TEntity> GetSet<TEntity>() where TEntity : class
        {
            Type key = typeof(TEntity);


            if (!_cache.TryGetValue(key, out IListSource value))
            {
                byte[] contentBytes = _fileProvider.GetBytes(GetDocumentsName(key.Name));
                if (Encrypted && contentBytes.Length > 0)
                {
                    contentBytes = _encryptionService.Decrypt(contentBytes);
                }
                string content = contentBytes.Length == 0 ? string.Empty : Encoding.UTF8.GetString(contentBytes);
                JsonTypeInfo listTypeInfo = _jsonOptions.GetTypeInfo(typeof(List<TEntity>));
                List<TEntity> list = string.IsNullOrWhiteSpace(content)
                    ? new List<TEntity>()
                    : (List<TEntity>)JsonSerializer.Deserialize(content, listTypeInfo) ?? new List<TEntity>();
                MemoryStoreSet<TEntity> entities = new MemoryStoreSet<TEntity>(list);
                _cache[key] = entities;
                return entities;
            }
            return (IStoreSet<TEntity>)value;
        }
        public void SaveChanges()
        {
            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
            foreach (KeyValuePair<Type, IListSource> kvp in _cache)
            {
                IList list = kvp.Value.GetList();
                JsonTypeInfo typeInfo = _jsonOptions.GetTypeInfo(list.GetType());
                string json = JsonSerializer.Serialize(list, typeInfo);
                byte[] data = Encoding.UTF8.GetBytes(json);
                if (Encrypted && data.Length > 0)
                {
                    data = _encryptionService.Encrypt(data);
                }
                files.Add(GetDocumentsName(kvp.Key.Name), data);
            }
            _fileProvider.WriteFiles(files);
        }

        public void DiscardChanges()
        {
            _cache.Clear();
        }



        public void EnableEncryption()
        {
            if (Encrypted) return;


            byte[] packageData = _encryptionService.CreatePackageInMemory();

            Dictionary<string, byte[]> files = new Dictionary<string, byte[]>
            {
                ["EncryptedPackage"] = packageData
            };

            byte[] manifestContent = _fileProvider.GetBytes("/Manifest.json");
            if (manifestContent.Length > 0)
            {
                files["/Manifest.json"] = _encryptionService.Encrypt(manifestContent);
            }

            IEnumerable<string> docFiles = _fileProvider.GetFileNames()
                .Where(f => f.StartsWith("documents/"));

            foreach (string file in docFiles)
            {
                byte[] content = _fileProvider.GetBytes($"/{file}");
                if (content.Length > 0)
                {
                    byte[] encrypted = _encryptionService.Encrypt(content);
                    files[$"/{file}"] = encrypted;
                }
            }

            _fileProvider.WriteFiles(files);
            Encrypted = true;
        }
    }
}
