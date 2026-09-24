namespace Toolkit.Modules.Azure.Models.KeyVaults
{
    public class PfxPasswordValidationOptions : ValidationOptions, IPasswordValidationOptions
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        public string PasswordKey { get; set; }

        #endregion
    }
}
