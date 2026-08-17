using AzureTools.Properties;
using System;
using System.Security.Cryptography.X509Certificates;

namespace AzureTools.Services.KeyVaults.Validators
{
    public class CertKeyValidator : IKeyValidator
    {
        public string Type => "Cert";
        public string ErrorMessage => MessageResources.ValidatorCertInvalid;
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
