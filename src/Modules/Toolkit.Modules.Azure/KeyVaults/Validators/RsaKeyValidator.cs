using System;
using System.Security.Cryptography;
using Toolkit.Modules.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class RsaKeyValidator : IKeyValidator
    {
        public string Type => "Rsa";
        public string ErrorMessage => MessageResources.ValidatorRsaInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;

            try
            {
                byte[] buffer = Convert.FromBase64String(value);
                string text = System.Text.Encoding.UTF8.GetString(buffer);
                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
