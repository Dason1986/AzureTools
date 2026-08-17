namespace AzureTools.Tools.Encryptions
{
    public interface IEncryption
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
