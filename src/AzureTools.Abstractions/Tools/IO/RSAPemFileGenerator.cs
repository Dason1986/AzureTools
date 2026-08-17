using System;
using System.Security.Cryptography;

namespace AzureTools.Tools.IO
{
    public class RSAPemFileGenerator : IFileGenerator
    {
        public IFileEntry[] Build(RsaFileOption option)
        {
            using RSA rsa = RSA.Create();
            string privateKey = rsa.ExportPkcs8PrivateKeyPem();
            string publicKey = rsa.ExportSubjectPublicKeyInfoPem();
            return new IFileEntry[]
            {
                new MemoryFileEntry("private.pem", privateKey),
                new MemoryFileEntry("public.pem", publicKey)
            };
        }

        IFileEntry[] IFileGenerator.Build(IFileGeneratorOption option)
        {
            if (option is RsaFileOption rsa)
                return Build(rsa);
            throw new NotSupportedException();
        }
    }
}
