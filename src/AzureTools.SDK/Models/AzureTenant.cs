namespace AzureTools.SDK.Models
{
    public class AzureTenant
    {
        public string TenantId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public AzureCredential Credential { get; set; }

        public string ClientId { get; set; }
        public string CertificateRawData { get; set; }
        public string Password { get; set; }
    }
}
