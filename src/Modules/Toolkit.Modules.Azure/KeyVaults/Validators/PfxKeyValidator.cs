using System;
using System.Security.Cryptography.X509Certificates;
using Toolkit.Modules.Azure.Models.KeyVaults;
using Toolkit.Modules.Azure.Properties;

namespace Toolkit.Modules.Azure.KeyVaults.Validators
{
    public class PfxKeyValidator : IKeyValidator
    {
        public string Type => "Pfx";
        public string ErrorMessage => AzureResource.ValidatorPfxInvalid;
        public bool Validate(string value, ValidatorContext context)
        {
            if (string.IsNullOrEmpty(value)) return false;

            try
            {
                byte[] buffer = Convert.FromBase64String(value);
                string password = context?.Get<string>("password");


                if (context?.Options is IPasswordValidationOptions pfxOpt && !string.IsNullOrEmpty(pfxOpt.PasswordKey) && password == null)
                    throw new NotSupportedException(string.Format(AzureResource.ValidatorPfxPasswordKeyNotFound, pfxOpt.PasswordKey));

                X509CertificateLoader.LoadPkcs12(buffer, password);
                return true;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }
    }
}
