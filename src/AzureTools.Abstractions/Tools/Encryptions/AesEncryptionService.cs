using AzureTools.Tools.IO;
using System;
using System.IO;
using System.Security.Cryptography;

namespace AzureTools.Tools.Encryptions
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly IStoreFileProvider _fileProvider;
        private readonly string _packagePath = "EncryptedPackage";
        private byte[] _key;
        private byte[] _iv;

        public AesEncryptionService(IStoreFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public bool LoadPackage()
        {
            byte[] content = _fileProvider.GetBytes(_packagePath);
            if (content == null || content.Length == 0)
            {
                return false;
            }
            LoadPackage(content);
            return true;
        }

        public byte[] CreatePackageInMemory()
        {
            return GenerateEncryptedPackage();
        }

        public byte[] GenerateEncryptedPackage()
        {
            using Aes aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            _key = aes.Key;
            _iv = aes.IV;

            byte[] keyIvData = new byte[KeySize + IvSize];
            Buffer.BlockCopy(_key, 0, keyIvData, 0, KeySize);
            Buffer.BlockCopy(_iv, 0, keyIvData, KeySize, IvSize);

            using MemoryStream memory = new MemoryStream();
            memory.Write(BitConverter.GetBytes((ulong)keyIvData.Length), 0, 8);
            memory.Write(keyIvData, 0, keyIvData.Length);
            return memory.ToArray();
        }

        public byte[] Encrypt(byte[] data)
        {
            EnsureKeys();

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            using MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }
            return ms.ToArray();
        }

        public byte[] Decrypt(byte[] data)
        {
            EnsureKeys();

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using MemoryStream ms = new MemoryStream(data);
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (MemoryStream result = new MemoryStream())
            {
                cs.CopyTo(result);
                return result.ToArray();
            }
        }

        private void LoadPackage(byte[] content)
        {
            if (content.Length < 8)
                throw new InvalidOperationException("EncryptedPackage 格式无效：数据长度不足");

            ulong dataSize = BitConverter.ToUInt64(content, 0);
            if ((ulong)content.Length < 8 + dataSize)
                throw new InvalidOperationException("EncryptedPackage 格式无效：数据不完整");

            byte[] keyIvData = new byte[(int)dataSize];
            Buffer.BlockCopy(content, 8, keyIvData, 0, (int)dataSize);

            if (keyIvData.Length < KeySize + IvSize)
                throw new InvalidOperationException("EncryptedPackage 格式无效：密钥数据不足");

            _key = new byte[KeySize];
            _iv = new byte[IvSize];
            Buffer.BlockCopy(keyIvData, 0, _key, 0, KeySize);
            Buffer.BlockCopy(keyIvData, KeySize, _iv, 0, IvSize);
        }

        private void EnsureKeys()
        {
            if (_key == null || _iv == null)
                LoadPackage();
        }

        private const int KeySize = 32;
        private const int IvSize = 16;
    }
}
