using System;
using System.Security.Cryptography.X509Certificates;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class CertKeyValidator : IKeyValidator
    {
        public string Type => "Cert";
        public string ErrorMessage => AzureResource.ValidatorCertInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;

            try
            {
                byte[] buffer = Convert.FromBase64String(value);
                X509CertificateLoader.LoadCertificate(buffer);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
