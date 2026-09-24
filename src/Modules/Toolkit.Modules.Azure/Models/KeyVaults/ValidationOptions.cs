

using System.Text.Json.Serialization;

namespace Toolkit.Modules.Azure.Models.KeyVaults
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
    [JsonDerivedType(typeof(DefaultValidationOptions), "Default")]
    [JsonDerivedType(typeof(PfxPasswordValidationOptions), "Pfx_Password")]
    [JsonDerivedType(typeof(NumberValidationOptions), "Number")]
    public abstract class ValidationOptions
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        public string Type { get; set; }
        public string ErrorMessage { get; set; }

        #endregion
    }
}
