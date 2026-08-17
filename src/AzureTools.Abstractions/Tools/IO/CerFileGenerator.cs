using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AzureTools.Tools.IO
{
    public class CerFileGenerator : IFileGenerator
    {

        public IFileEntry[] Build(CerFileOption option)
        {
            RSA rsa = RSA.Create();

            CertificateRequest request = new CertificateRequest(
                $"CN={option.Name}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

            DateTimeOffset notBefore = DateTimeOffset.UtcNow;
            DateTimeOffset notAfter = notBefore.AddYears(option.Year);

            X509Certificate2 cert = request.CreateSelfSigned(notBefore, notAfter);

            byte[] pfxData = cert.Export(X509ContentType.Pfx, option.Password);

            byte[] cerData = cert.Export(X509ContentType.Cert);
            return new IFileEntry[] {
                new MemoryFileEntry($"{option.Name}.pfx",pfxData),
                new MemoryFileEntry($"{option.Name}.cer",cerData),
            };
        }

        IFileEntry[] IFileGenerator.Build(IFileGeneratorOption option)
        {
            if (option is CerFileOption cer)
            {
                return Build(cer);
            }
            throw new NotSupportedException();
        }
    }
}
