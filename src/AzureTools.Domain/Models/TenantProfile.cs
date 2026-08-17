using System.ComponentModel.DataAnnotations;

namespace AzureTools.Models
{
    public class TenantProfile
    {
        #region Constructors

        #endregion

        #region Fields & Properties

        [Key]
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Credential { get; set; }

        public string ClientId { get; set; }
        public string CertificateRawData { get; set; }
        public string Password { get; set; }


        #endregion
    }
}
