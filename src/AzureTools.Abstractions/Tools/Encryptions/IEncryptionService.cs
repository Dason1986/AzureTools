namespace AzureTools.Tools.Encryptions
{
    public interface IEncryptionService : IEncryption
    {
        byte[] GenerateEncryptedPackage();

        bool LoadPackage();

        byte[] CreatePackageInMemory();
    }
}
